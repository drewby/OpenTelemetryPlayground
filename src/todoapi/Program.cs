using System.Text;
using Microsoft.AspNetCore.Authentication;
using Ops;

var builder = WebApplication.CreateBuilder(args);

// This initializes the Observability features in the ops SDK project
builder.AddOps(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TodoApi", Version = "v1" });
    var filePath = Path.Combine(System.AppContext.BaseDirectory, "todoapi.xml");
    c.IncludeXmlComments(filePath);
});

// Add a startup service which will run in the background and then signal the app is ready for requests
builder.Services.AddHostedService<StartupBackgroundService>();

// Add a Dapr client and Todo service
builder.Services.AddSingleton<Dapr.Client.DaprClient>(new Dapr.Client.DaprClientBuilder().Build());
builder.Services.AddSingleton<TodoService>();

// Add a utility for getting the current httpContext
builder.Services.AddHttpContextAccessor();

// Build the application using the services and configuration
var app = builder.Build();

// This configures the observability features in the ops SDK project
app.ConfigOps();

// Enable Swagger at /swagger/index.html
app.UseSwagger();
app.UseSwaggerUI();

// Map the default routes to the TodoController and AuthController
app.MapControllers();

// Run the application
app.Run();
