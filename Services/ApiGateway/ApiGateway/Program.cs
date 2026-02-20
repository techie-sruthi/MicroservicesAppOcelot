using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration - use Docker config when running in container
var ocelotFile = Environment.GetEnvironmentVariable("OCELOT_CONFIG") ?? "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

// Add Ocelot services
builder.Services.AddOcelot();

// Add CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAngular");

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
