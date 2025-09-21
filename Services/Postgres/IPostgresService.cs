namespace Craftmatrix.org.Services
{
    public interface IPostgresService
    {
        public string DebugString();
        public Task<T> DebugFunction<T>(T data);
    }
}
