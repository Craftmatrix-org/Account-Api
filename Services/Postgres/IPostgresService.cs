namespace Craftmatrix.org.Services
{
    public interface IPostgresService
    {
        Task<int> PostDataAsync<T>(T data, string table);
        Task<bool> UpdateDataAsync<T>(T data, object id, string table);
        Task<bool> DeleteDataAsync(object id, string table);
        Task<T?> GetDataAsync<T>(object id, string table);
        Task<IEnumerable<T>> IndexSearchAsync<T>(string column, object value, string table);
        Task<IEnumerable<T>> RecursiveSearchAsync<T>(Dictionary<string, object> criteria, string table);
        Task<IEnumerable<T>> GetAllAsync<T>(string table);
        
        string DebugString();
        Task<T> DebugFunction<T>(T data);
    }
}
