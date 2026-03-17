using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Http;
using ApiGateway;
using ApiGateway.Exceptions;
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
var keyString = jwtSection["Key"] ?? throw new ConfigurationMissingException("JWT Key");
var issuer = jwtSection["Issuer"] ?? throw new ConfigurationMissingException("JWT Issuer");
var audience = jwtSection["Audience"] ?? throw new ConfigurationMissingException("JWT Audience");

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

builder.Services
    .AddOcelot(builder.Configuration)
    .AddSingletonDefinedAggregator<ProductUserAggregator>();

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
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

await app.UseOcelot();

await app.RunAsync();
