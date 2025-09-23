using Craftmatrix.org.Services;
using Craftmatrix.org.DB;
using Microsoft.EntityFrameworkCore;
using Craftmatrix.org.ConnString;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

DotNetEnv.Env.Load();
DotNetEnv.Env.TraversePath().Load();


var builder = WebApplication.CreateBuilder(args);

var Key = DotNetEnv.Env.GetString("KEY");
var Issuer = DotNetEnv.Env.GetString("ISSUER");
var Audience = DotNetEnv.Env.GetString("AUDIENCE");

builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token. Example: \"Bearer 12345abcdef\""
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          }
        },
        Array.Empty<string>()
    }
    });
});
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false; // -> must be true in production;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))

    };

});

builder.Services.AddEndpointsApiExplorer();

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
