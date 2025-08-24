using System.ClientModel.Primitives;
using AgentFunction.Functions.Agents;
using AgentFunction.Functions.Plugins;

using Azure.AI.OpenAI;
using Azure.Communication.Email;

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

using Shared;

var builder = FunctionsApplication.CreateBuilder(args);

// Reference the Aspire ServiceDefaults project to add default services.
builder.AddServiceDefaults();

builder.AddAzureQueueServiceClient(Services.AzureStorageQueues);
builder.AddAzureBlobServiceClient(Services.AzureStorageBlobs);

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

AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

// Enable diagnostics.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnostics", true);

// Uncomment the following line to enable diagnostics with sensitive data: prompts, completions, function calls, and more.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

// Enable SK traces using OpenTelemetry.Extensions.Hosting extensions.
// An alternative approach to enabling traces can be found here: https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/observability/telemetry-with-aspire-dashboard?tabs=Powershell&pivots=programming-language-csharp 
builder.Services.AddOpenTelemetry().WithTracing(b => b.AddSource("Microsoft.SemanticKernel*"));

// Enable SK metrics using OpenTelemetry.Extensions.Hosting extensions.
// An alternative approach to enabling metrics can be found here: https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/observability/telemetry-with-aspire-dashboard?tabs=Powershell&pivots=programming-language-csharp
builder.Services.AddOpenTelemetry().WithMetrics(b => b.AddMeter("Microsoft.SemanticKernel*"));



builder.AddAzureOpenAIClient(
    connectionName: Services.AzureOpenAI,
    configureClientBuilder: clientBuilder =>
    {
        clientBuilder.ConfigureOptions((options) =>
        {
            // Set maxRetries to 3 to balance resilience against transient failures and avoid excessive delays.
            options.RetryPolicy = new ClientRetryPolicy(maxRetries: 3);
        });
    }
);


builder.Services.AddSingleton<Kernel>(sp =>
{
    var client = sp.GetRequiredService<AzureOpenAIClient>();

    var kernel = Kernel.CreateBuilder();

    var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
                        ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME environment variable is not set.");


    kernel.AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        azureOpenAIClient: client
    );

    // TODO: Add plugins here
    kernel.Plugins.AddFromType<SchemaTools>("SchemaTools");
    kernel.Plugins.AddFromType<PriorClaimsTools>("PriorClaimsTools");
    kernel.Plugins.AddFromType<PolicyTools>("PolicyTools");

    return kernel.Build();
});

builder.Services.AddSingleton<CompletenessAgent>();
builder.Services.AddSingleton<CanonicalizeAgent>();
builder.Services.AddSingleton<CoverageAgent>();
builder.Services.AddSingleton<FraudAgent>();
builder.Services.AddSingleton<CommsAgent>();

builder.Build().Run();
