using Azure.Communication.Email;

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shared;

var builder = FunctionsApplication.CreateBuilder(args);

// Reference the Aspire ServiceDefaults project to add default services.
builder.AddServiceDefaults();

builder.AddAzureQueueServiceClient(Services.AzureStorageQueues);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient("claimsagent", (client) =>
{
    client.BaseAddress = new($"https+http://{Services.ClaimsAgentService}");
});

string? acsConnString = builder.Configuration.GetConnectionString("AzureCommunicationServiceConnectionString");
builder.Services.AddSingleton(sp =>
{
    if (string.IsNullOrEmpty(acsConnString))
    {
        throw new InvalidOperationException("Azure Communication Service connection string is not configured.");
    }
    return new EmailClient(acsConnString);
});

builder.Build().Run();
