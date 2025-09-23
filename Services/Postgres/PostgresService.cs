using Craftmatrix.org.ConnString;
using Npgsql;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Craftmatrix.org.Services
{
    public class PostgresService : IPostgresService
    {
        private readonly string _connectionString;
        private readonly ConnStrings _connString;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
        private static readonly Regex _tableNameValidator = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

        public PostgresService()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
            _connString = new ConnStrings();
            _connectionString = _connString.PostGres();
        }

        private void ValidateTableName(string table)
        {
            if (string.IsNullOrWhiteSpace(table) || !_tableNameValidator.IsMatch(table))
                throw new ArgumentException("Invalid table name format", nameof(table));
        }

        private PropertyInfo[] GetCachedProperties<T>()
        {
            return _propertyCache.GetOrAdd(typeof(T), t => t.GetProperties());
        }

        public async Task<Guid> PostDataAsync<T>(T data, string table)
        {
            try
            {
                ValidateTableName(table);

                // Only primitive or string properties (skip navigation references)
                var properties = GetCachedProperties<T>()
    .Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(Guid) || p.PropertyType == typeof(DateTime))
    .ToList();


                var columns = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
                var values = string.Join(", ", properties.Select((p, i) => $"@param{i}"));

                // Handle Id
                var idProperty = typeof(T).GetProperty("Id");
                Guid idValue = Guid.NewGuid();

                if (idProperty != null)
                {
                    var currentId = (Guid)idProperty.GetValue(data);
                    if (currentId == Guid.Empty)
                        idProperty.SetValue(data, idValue);
                    else
                        idValue = currentId;

                    columns = "\"Id\", " + columns;
                    values = "@id, " + values;
                }

                var sql = $"INSERT INTO \"{table}\" ({columns}) VALUES ({values}) RETURNING \"Id\"";

                using var conn = new NpgsqlConnection(_connectionString);
                using var cmd = new NpgsqlCommand(sql, conn);

                if (idProperty != null)
                    cmd.Parameters.AddWithValue("@id", idValue);

                for (int i = 0; i < properties.Count; i++)
                {
                    var val = properties[i].GetValue(data) ?? DBNull.Value;
                    cmd.Parameters.AddWithValue($"@param{i}", val);
                }

                await conn.OpenAsync();
                await cmd.ExecuteScalarAsync();

                return idValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert into '{table}': {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateDataAsync<T>(T data, object id, string table)
        {
            ValidateTableName(table);
            var properties = GetCachedProperties<T>()
                .Where(p => p.Name.ToLower() != "id" && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                .ToList();

            var setClause = string.Join(", ", properties.Select((p, i) => $"\"{p.Name}\" = @param{i}"));
            var sql = $"UPDATE \"{table}\" SET {setClause} WHERE id = @id";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);
            for (int i = 0; i < properties.Count; i++)
            {
                var val = properties[i].GetValue(data) ?? DBNull.Value;
                cmd.Parameters.AddWithValue($"@param{i}", val);
            }

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteDataAsync(object id, string table)
        {
            ValidateTableName(table);
            var sql = $"DELETE FROM \"{table}\" WHERE id = @id";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<T?> GetDataAsync<T>(object id, string table)
        {
            ValidateTableName(table);
            var sql = $"SELECT * FROM \"{table}\" WHERE id = @id LIMIT 1";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapToObject<T>(reader);

            return default;
        }

        public async Task<IEnumerable<T>> IndexSearchAsync<T>(string column, object value, string table)
        {
            ValidateTableName(table);
            if (string.IsNullOrWhiteSpace(column) || !_tableNameValidator.IsMatch(column))
                throw new ArgumentException("Invalid column name", nameof(column));

            var sql = $"SELECT * FROM \"{table}\" WHERE \"{column}\" = @value";
            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@value", value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<T>();
            while (await reader.ReadAsync())
                results.Add(MapToObject<T>(reader));

            return results;
        }

        public async Task<IEnumerable<T>> RecursiveSearchAsync<T>(Dictionary<string, object> criteria, string table)
        {
            ValidateTableName(table);
            foreach (var key in criteria.Keys)
            {
                if (string.IsNullOrWhiteSpace(key) || !_tableNameValidator.IsMatch(key))
                    throw new ArgumentException($"Invalid column name: {key}", nameof(criteria));
            }

            var whereClause = string.Join(" AND ", criteria.Select((kvp, i) => $"\"{kvp.Key}\" = @param{i}"));
            var sql = $"SELECT * FROM \"{table}\" WHERE {whereClause}";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);

            int iParam = 0;
            foreach (var kvp in criteria)
                cmd.Parameters.AddWithValue($"@param{iParam++}", kvp.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<T>();
            while (await reader.ReadAsync())
                results.Add(MapToObject<T>(reader));

            return results;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string table)
        {
            ValidateTableName(table);
            var sql = $"SELECT * FROM \"{table}\"";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<T>();
            while (await reader.ReadAsync())
                results.Add(MapToObject<T>(reader));

            return results;
        }

        private T MapToObject<T>(NpgsqlDataReader reader)
        {
            var obj = Activator.CreateInstance<T>();
            var properties = GetCachedProperties<T>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.CanWrite && !reader.IsDBNull(i))
                {
                    var val = reader.GetValue(i);
                    if (val != null)
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        if (targetType.IsEnum)
                            prop.SetValue(obj, Enum.Parse(targetType, val.ToString()));
                        else if (targetType == typeof(Guid))
                            prop.SetValue(obj, Guid.Parse(val.ToString()));
                        else
                            prop.SetValue(obj, Convert.ChangeType(val, targetType));
                    }
                }
            }

            return obj;
        }

        public string DebugString() => _connString.PostGres();

        public Task<T> DebugFunction<T>(T data) => Task.FromResult(data);
    }
}

