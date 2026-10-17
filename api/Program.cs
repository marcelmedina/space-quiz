using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaceQuiz.Api;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(sp => CreateTableClient(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<ScoreRepository>();

builder.Build().Run();

// Reuses the Function App's own storage account. Supports both a connection string
// and identity-based connections (AzureWebJobsStorage__accountName / __tableServiceUri).
static TableClient CreateTableClient(IConfiguration config)
{
    var connectionString = config["AzureWebJobsStorage"];
    if (!string.IsNullOrWhiteSpace(connectionString))
        return new TableClient(connectionString, ScoreRepository.TableName);

    var section = config.GetSection("AzureWebJobsStorage");
    var serviceUri = section["tableServiceUri"] is { Length: > 0 } uri
        ? new Uri(uri)
        : section["accountName"] is { Length: > 0 } account
            ? new Uri($"https://{account}.table.core.windows.net")
            : throw new InvalidOperationException("AzureWebJobsStorage is not configured.");

    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = section["clientId"]
    });
    return new TableServiceClient(serviceUri, credential).GetTableClient(ScoreRepository.TableName);
}
