namespace Craftmatrix.org.Services
{
     public class PostgresService : IPostgresService
    {
        public string ConnString = "";

        public PostgresService()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();

        }

        public string DebugString()
        {

            var envi = DotNetEnv.Env.GetString("ENVIRONMENT");
            return envi;
        }
        
    }
}