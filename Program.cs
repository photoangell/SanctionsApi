using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

const string CorsOriginsSetup = "SanctionsApiOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsOriginsSetup,
        builder => { builder.AllowAnyOrigin(); });
});
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(CorsOriginsSetup);
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();