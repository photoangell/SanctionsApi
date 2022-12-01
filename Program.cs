using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SanctionsApi.Models;
using SanctionsApi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.Configure<List<SanctionsListConfig>>(builder.Configuration.GetSection("SanctionLists"));

const string CorsOriginsSetup = "SanctionsApiOrigins";
builder.Services.AddCors(options => options.AddPolicy(CorsOriginsSetup,
    builder =>
    {
        builder
            .WithOrigins(AllowedOrigins())
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowCredentials();
    }));
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Sanctions Api", Version = "v1"
        });
    });
}

builder.Services.AddScoped<IBuildSanctionsReport, BuildSanctionsReport>()
    .AddScoped<INameMatcher, SimpleNameMatcher>();

var app = builder.Build();
app.UseCors(CorsOriginsSetup);
app.UseSerilogRequestLogging();
app.Logger.LogInformation("The app started");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
        $"{builder.Environment.ApplicationName} v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
app.Logger.LogInformation("The app terminated");

string[] AllowedOrigins()
{
    if (builder.Environment.IsDevelopment())
    {
        return new[] { "http://localhost", "https://localhost:5001" };
    }

    return new[]
    {
        "https://*.conveyancinginsurance.co.uk", "http://*.conveyancinginsurance.co.uk",
        "http://complaints.legal-contingency.co.uk", "https://uw.lcre.uk"
    };
}