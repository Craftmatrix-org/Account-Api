using Craftmatrix.org.Services;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// builder.Services.AddSingleton<IGreetingService, DefaultGreetingService>();
builder.Services.AddSingleton<IPostgresService, PostgresService>();

// var serviceProvider = builder.Services.BuildServiceProvider();
// var GreetService = serviceProvider.GetRequiredService<IGreetingService>();
// var Greeting = GreetService.Greet("World");
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
