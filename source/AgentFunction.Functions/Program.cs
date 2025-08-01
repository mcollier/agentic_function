using System.ClientModel;
using AgentFunction.Functions;
using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

var builder = FunctionsApplication.CreateBuilder(args);

// Reference the Aspire ServiceDefaults project to add default services.
builder.AddServiceDefaults();

// builder.AddAzureOpenAIClient("openai");

// builder.AddAzureChatCompletionsClient(connectionName: "chat")
//        .AddChatClient();

builder.ConfigureFunctionsWebApplication();

string deploymentName = "gpt-4o-mini";
string serviceKey = "ClaimsProcessingAgent";

IChatClient chatClient;

// TODO: Use Azure Entra ID authentication instead of API key in production
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

// Setting up MCP
var clientTransport = new SseClientTransport(new SseClientTransportOptions
{
    Endpoint = new Uri("http://localhost:5476"),
    TransportMode = HttpTransportMode.AutoDetect,
});
var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
var mcpTools = await mcpClient.ListToolsAsync();
foreach (var tool in mcpTools)
{
    Console.WriteLine($"Tool: {tool.Name} ({tool.Description})");
}

// test execute mcp
var mcpResult = await mcpClient.CallToolAsync(
    "GetClaimsHistory",
    new Dictionary<string, object?> { ["customerId"] = "12345" }
    , cancellationToken: CancellationToken.None);

Console.WriteLine($"MCP Result: {mcpResult.IsError} {mcpResult.Content[0]}");

chatClient = new AzureOpenAIClient(endpoint: new Uri(endpoint),
                                   credential: new ApiKeyCredential(apiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient();
var functionCallingChatClient = chatClient.AsBuilder().UseKernelFunctionInvocation().Build();
builder.Services.AddTransient<IChatClient>(sp => functionCallingChatClient);

builder.Services.AddTransient<Kernel>();

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

builder.Services.AddSingleton<ClaimsProcessingPlugin>();

builder.Services.AddKeyedSingleton(
    serviceKey,
    (sp, key) => 
    {
        KernelPluginCollection plugins = [];
        plugins.AddFromObject(sp.GetRequiredService<ClaimsProcessingPlugin>());

        var kernel = sp.GetRequiredService<Kernel>().Clone();
        kernel.Plugins.AddFromType<ClaimsProcessingPlugin>("ClaimsProcessingPlugin");

                // Add MCP tools as functions
        kernel.Plugins.AddFromFunctions("ClaimHistory", mcpTools.Select(aiFunction => aiFunction.AsKernelFunction()));

        return new ChatCompletionAgent()
        {
            Instructions = """
                           You are an agent that processes insurance claims. You will validate the completeness of claims.
                           If the claim is complete, return true. If not, return false.                          
                           """,
            Description = "An agent that validates the completeness of insurance claims.",
            Name = serviceKey,
            Kernel = kernel,
            Arguments = new KernelArguments(openAIPromptExecutionSettings)
        };
    });

builder.Services.BuildServiceProvider();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
