using Azure.Communication.Email;
using Azure.Identity;

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

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

// Load AgentSettings from configuration
// builder.Services.AddOptions<AgentSettings>().BindConfiguration("Agents");

builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernel = Kernel.CreateBuilder();

    // string aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");

    kernel.AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o-mini",
        // deploymentName: "dummy-deployment",
        endpoint: "https://oai-agenticfunction.openai.azure.com/",
        credentials: new DefaultAzureCredential());

    // TODO: Add plugins here
    kernel.Plugins.AddFromType<Shared.Agents.Tools.SchemaTools>("SchemaTools");

    return kernel.Build();
});

builder.Services.AddSingleton<CompletenessAgent>();

builder.Build().Run();
