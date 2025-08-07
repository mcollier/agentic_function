using System.ClientModel.Primitives;
using AgentFunction.ClaimsAgent.Plugins;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

// Add service defaults and Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

// TODO:Load the service configuration.

builder.Services.AddKernel();

// Set up Semantic Kernel logging.
// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);


// Add AI Services.
AddAIServices(builder);

// Add Agent.
await AddAgent(builder);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("Microsoft.SemanticKernel");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();

static void AddAIServices(WebApplicationBuilder builder)
{
    builder.AddAzureOpenAIClient(
        connectionName: Shared.Services.AzureOpenAI,
        configureSettings: (settings) => settings.Credential = new DefaultAzureCredential(),
        // configureSettings: (settings) => settings.Credential = builder.Environment.IsProduction()
        //             ? new DefaultAzureCredential()
        //             : new AzureCliCredential(),
        configureClientBuilder: clientBuilder =>
        {
            clientBuilder.ConfigureOptions((options) =>
            {
                options.RetryPolicy = new ClientRetryPolicy(maxRetries: 3);
            });
        });

    builder.Services.AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o-mini");
}

static async Task AddAgent(WebApplicationBuilder builder)
{
    string template = """
                      You are an agent tasked with processing automobile insurance claims.
                      You have access to tool to help you check the completeness of a claim, and check if a claim is fraudulent.
                      You can also summarize the claim details using the provided claim information.
                      """;

    PromptTemplateConfig promptTemplateConfig = new()
    {
        Template = template,
        TemplateFormat = "semantic-kernel",
        Name = "ClaimProcessAgentPrompt"
    };

    // Enable planning
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };

    // MCP tools
    string mcpServerUrl = Environment.GetEnvironmentVariable("MCP_SERVER_URL") ??
                            throw new InvalidOperationException("MCP_SERVER_URL environment variable is not set.");
    var clientTransport = new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri(mcpServerUrl),
        TransportMode = HttpTransportMode.AutoDetect
    });
    var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
    var mcpTools = await mcpClient.ListToolsAsync();

    builder.Services.AddTransient((sp) =>
    {
        var kernel = sp.GetRequiredService<Kernel>();
        kernel.Plugins.AddFromType<ClaimsProcessingPlugin>("ClaimsProcessingPlugin");

        // Add MCP tools!
        kernel.Plugins.AddFromFunctions("ClaimHistory", mcpTools.Select(aiFunction => aiFunction.AsKernelFunction()));

        return new ChatCompletionAgent(promptTemplateConfig, new KernelPromptTemplateFactory())
        {
            Kernel = kernel,
            Arguments = new KernelArguments(openAIPromptExecutionSettings)
        };
    });
}