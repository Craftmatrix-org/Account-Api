using Craftmatrix.org.ConnString;

namespace Craftmatrix.org.Services
{
    public class PostgresService : IPostgresService
    {
        public string ConnString = "";

        ConnStrings connString;

        public PostgresService()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
            connString = new ConnStrings();
        }

        public string DebugString()
        {
            return connString.PostGres();
        }

        public Task<T> DebugFunction<T>(T data)
        {
            return Task.FromResult(data);
        }
    }
}
