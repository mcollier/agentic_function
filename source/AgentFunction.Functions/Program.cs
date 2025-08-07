using Microsoft.Azure.Functions.Worker.Builder;
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


builder.Build().Run();
