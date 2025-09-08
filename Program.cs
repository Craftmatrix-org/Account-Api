using Craftmatrix.org.Services;
using Craftmatrix.org.DB;
using Microsoft.EntityFrameworkCore;
using Craftmatrix.org.ConnString;
using Asp.Versioning.ApiExplorer;

DotNetEnv.Env.Load();
DotNetEnv.Env.TraversePath().Load();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddControllers();

builder.Services.AddApiVersioning(opt =>
{
    opt.ReportApiVersions = true;
}).AddApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'VVV";
    opt.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSingleton<IPostgresService, PostgresService>();

ConnStrings connectionString = new ConnStrings();

builder.Services.AddDbContextPool<UserContext>(opts =>
    opts.UseNpgsql(connectionString.PostGres()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(e =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            e.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()
                );
        }
        e.InjectStylesheet("/swagger-ui/SwaggerDark.css");
    });
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
