using Craftmatrix.org.Services;
using Craftmatrix.org.DB;
using Microsoft.EntityFrameworkCore;
using Craftmatrix.org.ConnString;
DotNetEnv.Env.Load();
DotNetEnv.Env.TraversePath().Load();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<IPostgresService, PostgresService>();

ConnStrings connectionString = new ConnStrings();

builder.Services.AddDbContextPool<UserContext>(opts =>
    opts.UseNpgsql(connectionString.PostGres()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
