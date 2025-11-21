namespace Craftmatrix.org.ConnString
{
    public class ConnStrings
    {
        public ConnStrings()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
        }
        public string PostGres()
        {
            var user = DotNetEnv.Env.GetString("POSTGRES_USERNAME");
            var host = DotNetEnv.Env.GetString("POSTGRES_HOST");
            var port = DotNetEnv.Env.GetString("POSTGRES_PORT");
            var database = DotNetEnv.Env.GetString("POSTGRES_DATABASE");
            var password = DotNetEnv.Env.GetString("POSTGRES_PASS");
            var envi = DotNetEnv.Env.GetString("ENVIRONMENT");

            return $"Username={user};Password={password};Host={host};Port={port};Database={database}";

        }

    }
}
