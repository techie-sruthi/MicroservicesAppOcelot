using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Http;
using ApiGateway;
using ApiGateway.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using ApiGateway.Aggregator;
using Microsoft.IdentityModel.Logging;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

var ocelotFile = Environment.GetEnvironmentVariable("OCELOT_CONFIG") ?? "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = jwtSection["Key"] ?? throw new Exception("JWT Key missing in configuration");
var issuer = jwtSection["Issuer"] ?? throw new Exception("JWT Issuer missing in configuration");
var audience = jwtSection["Audience"] ?? throw new Exception("JWT Audience missing in configuration");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

//builder.Services.AddHttpClient();

builder.Services
    .AddOcelot(builder.Configuration)
    .AddSingletonDefinedAggregator<ProductUserAggregator>();

//builder.Services.AddOcelot();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

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

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
//app.Use(async (context, next) =>
//{
//    if (context.User.Identity?.IsAuthenticated == true)
//    {
//        foreach (var claim in context.User.Claims)
//        {
//            Console.WriteLine($"Claim *****: {claim.Type} = {claim.Value}");
//        }
//    }
//    await next();
//});
//app.UseMiddleware<RateLimitingMiddleware>();
await app.UseOcelot();

app.Run();
