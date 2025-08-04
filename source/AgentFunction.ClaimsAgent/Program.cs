using System.ClientModel.Primitives;
using System.Threading.Tasks;
using AgentFunction.ClaimsAgent.Plugins;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
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

// Add AI Services.
AddAIServices(builder);

// Add Agent.
await AddAgent(builder);

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


// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }

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
    var clientTransport = new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5476"),
        TransportMode = HttpTransportMode.AutoDetect
    });
    var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
    var mcpTools = await mcpClient.ListToolsAsync();

    builder.Services.AddTransient((sp) =>
    {
        var kernel = sp.GetRequiredService<Kernel>();
        kernel.Plugins.AddFromType<ClaimsProcessingPlugin>("ClaimsProcessingPlugin");

        // TODO: Add MCP tools!
        kernel.Plugins.AddFromFunctions("ClaimHistory", mcpTools.Select(aiFunction => aiFunction.AsKernelFunction()));

        return new ChatCompletionAgent(promptTemplateConfig, new KernelPromptTemplateFactory())
        {
            Kernel = kernel,
            Arguments = new KernelArguments(openAIPromptExecutionSettings)
        };
    });
}