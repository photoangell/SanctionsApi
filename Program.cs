using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SanctionsApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<List<SanctionsListConfig>>(builder.Configuration.GetSection("SanctionLists"));

const string CorsOriginsSetup = "SanctionsApiOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsOriginsSetup,
        builder => { builder.AllowAnyOrigin(); });
});
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

var app = builder.Build();
app.Logger.LogInformation("The app started");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
        $"{builder.Environment.ApplicationName} v1"));
}

app.UseHttpsRedirection();
app.UseCors(CorsOriginsSetup);
app.MapControllers();

app.Run();
app.Logger.LogInformation("The app terminated");