using Craftmatrix.org.ConnString;
using Npgsql;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;
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
            {
                throw new ArgumentException("Invalid table name format", nameof(table));
            }
        }

        private PropertyInfo[] GetCachedProperties<T>()
        {
            return _propertyCache.GetOrAdd(typeof(T), type => type.GetProperties());
        }

        public async Task<int> PostDataAsync<T>(T data, string table)
        {
            try
            {
                ValidateTableName(table);
                var properties = GetCachedProperties<T>().Where(p => p.Name.ToLower() != "id");
                var columns = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
                var values = string.Join(", ", properties.Select((p, i) => $"@param{i}"));
                
                var sql = $"INSERT INTO \"{table}\" ({columns}) VALUES ({values}) RETURNING id";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                
                for (int i = 0; i < properties.Count(); i++)
                {
                    var value = properties.ElementAt(i).GetValue(data) ?? DBNull.Value;
                    command.Parameters.AddWithValue($"@param{i}", value);
                }
                
                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert data into table '{table}': {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateDataAsync<T>(T data, object id, string table)
        {
            try
            {
                ValidateTableName(table);
                var properties = GetCachedProperties<T>().Where(p => p.Name.ToLower() != "id");
                var setClause = string.Join(", ", properties.Select((p, i) => $"\"{p.Name}\" = @param{i}"));
                
                var sql = $"UPDATE \"{table}\" SET {setClause} WHERE id = @id";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                
                command.Parameters.AddWithValue("@id", id);
                for (int i = 0; i < properties.Count(); i++)
                {
                    var value = properties.ElementAt(i).GetValue(data) ?? DBNull.Value;
                    command.Parameters.AddWithValue($"@param{i}", value);
                }
                
                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update data in table '{table}': {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteDataAsync(object id, string table)
        {
            try
            {
                ValidateTableName(table);
                var sql = $"DELETE FROM \"{table}\" WHERE id = @id";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                
                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete data from table '{table}': {ex.Message}", ex);
            }
        }

        public async Task<T?> GetDataAsync<T>(object id, string table)
        {
            try
            {
                ValidateTableName(table);
                var sql = $"SELECT * FROM \"{table}\" WHERE id = @id LIMIT 1";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return MapToObject<T>(reader);
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get data from table '{table}': {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<T>> IndexSearchAsync<T>(string column, object value, string table)
        {
            try
            {
                ValidateTableName(table);
                if (string.IsNullOrWhiteSpace(column) || !_tableNameValidator.IsMatch(column))
                {
                    throw new ArgumentException("Invalid column name format", nameof(column));
                }
                
                var sql = $"SELECT * FROM \"{table}\" WHERE \"{column}\" = @value";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@value", value);
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                var results = new List<T>();
                while (await reader.ReadAsync())
                {
                    results.Add(MapToObject<T>(reader));
                }
                
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to search data in table '{table}': {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<T>> RecursiveSearchAsync<T>(Dictionary<string, object> criteria, string table)
        {
            try
            {
                ValidateTableName(table);
                foreach (var key in criteria.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || !_tableNameValidator.IsMatch(key))
                    {
                        throw new ArgumentException($"Invalid column name format: {key}", nameof(criteria));
                    }
                }
                
                var whereClause = string.Join(" AND ", criteria.Select((kvp, i) => $"\"{kvp.Key}\" = @param{i}"));
                var sql = $"SELECT * FROM \"{table}\" WHERE {whereClause}";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                
                int paramIndex = 0;
                foreach (var kvp in criteria)
                {
                    command.Parameters.AddWithValue($"@param{paramIndex}", kvp.Value);
                    paramIndex++;
                }
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                var results = new List<T>();
                while (await reader.ReadAsync())
                {
                    results.Add(MapToObject<T>(reader));
                }
                
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to recursive search in table '{table}': {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string table)
        {
            try
            {
                ValidateTableName(table);
                var sql = $"SELECT * FROM \"{table}\"";
                
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(sql, connection);
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                var results = new List<T>();
                while (await reader.ReadAsync())
                {
                    results.Add(MapToObject<T>(reader));
                }
                
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get all data from table '{table}': {ex.Message}", ex);
            }
        }

        private T MapToObject<T>(NpgsqlDataReader reader)
        {
            var obj = Activator.CreateInstance<T>();
            var properties = GetCachedProperties<T>();
            
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => 
                    string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                
                if (property != null && property.CanWrite && !reader.IsDBNull(i))
                {
                    var value = reader.GetValue(i);
                    if (value != null)
                    {
                        try
                        {
                            var targetType = property.PropertyType;
                            var underlyingType = Nullable.GetUnderlyingType(targetType);
                            
                            if (underlyingType != null)
                            {
                                targetType = underlyingType;
                            }
                            
                            if (targetType.IsEnum)
                            {
                                property.SetValue(obj, Enum.Parse(targetType, value.ToString()));
                            }
                            else if (targetType == typeof(Guid))
                            {
                                property.SetValue(obj, Guid.Parse(value.ToString()));
                            }
                            else
                            {
                                property.SetValue(obj, Convert.ChangeType(value, targetType));
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Failed to convert value '{value}' to type '{property.PropertyType.Name}' for property '{property.Name}': {ex.Message}", ex);
                        }
                    }
                }
            }
            
            return obj;
        }

        public string DebugString()
        {
            return _connString.PostGres();
        }

        public Task<T> DebugFunction<T>(T data)
        {
            return Task.FromResult(data);
        }
    }
}
