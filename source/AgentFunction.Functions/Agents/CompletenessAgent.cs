using System.Text.Json;
using System.Text.Json.Serialization;

using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

public sealed class CompletenessAgent : IAgent<FnolClaim, CompletenessResult>
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger<CompletenessAgent> _logger;
    // private readonly AgentModel _cfg;

    // public CompletenessAgent(Kernel kernel, IOptions<AgentSettings> settings)
    public CompletenessAgent(Kernel kernel, ILogger<CompletenessAgent> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // _cfg = settings.Value.Completeness;

        _agent = new ChatCompletionAgent()
        {
            Name = "CompletenessAgent",
            Instructions = @"You are an agent that checks the completeness of insurance claims.
            
            Goal:
            - Inspect a FNOL JSON payload.
            - Identify missing or inconsistent fields vs. the FNOL schema.
            - Generate clarifying questions for the customer to fill in missing information.
            - Use JSON Pointer paths to indicate missing fields, e.g., '/parties/0/contact/phone'.

            Tools:
            - SchemaTools.GetFnolSchemaAsync() to fetch the schema.
            - SchemaTools.GetEnumValuesAsync(field) for canonical enums.
            
            Output STRICT JSON ONLY:
            {
                ""missingFields"": [""/parties/0/contact/phone""],
                ""clarifyingQuestions"": [""What is the phone number for the first party's contact?""]
            }",
            Kernel = kernel,
            // Description = "",
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }
            )
        };
    }

    public async Task<CompletenessResult> ExecuteAsync(FnolClaim input, CancellationToken ct = default)
    {
        var fnolJson = JsonSerializer.Serialize(input, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        // var history = new ChatHistory();

        var content = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Analyze this FNOL JSON for missing/inconsistent fields and produce the required JSON output.\n" +
                     $"You may call functions to assist in your analysis.\n" +
                     $"You may call SchemaTools.GetFnolSchemaAsync() and SchemaTools.GetEnumValuesAsync(field).\n" +
                     $"FNOL JSON:\n" +
                     $"""
                     ```json
                     {fnolJson}
                     ```
                     """
        );

        // history.Add(content);

        var execSettings = new OpenAIPromptExecutionSettings
        {
            // ModelId = _cfg.ModelId,
            // Temperature = _cfg.Temperature,
            // TopP = _cfg.TopP,
            ModelId = "gpt-4o-mini",
            Temperature = 0.2f,
            TopP = 1.0f,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            // FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            ResponseFormat = "json_object"
            // ResponseFormat = typeof(CompletenessResult)
        };

        AgentInvokeOptions options = new()
        {
            KernelArguments = new KernelArguments(execSettings),
            Kernel = _agent.Kernel,
        };

        IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> response =
            _agent.InvokeAsync(message: content,
                               options: options,
                               cancellationToken: ct);

        // Process the agent's response. Only concerned with the first item.
        ChatMessageContent? chatMessageContent = null;
        await foreach (AgentResponseItem<ChatMessageContent> item in response.ConfigureAwait(false))
        {
            chatMessageContent = item.Message;
            break;
        }

        var usage = chatMessageContent?.Metadata?["Usage"] as OpenAI.Chat.ChatTokenUsage;

        // Log the raw agent response (may be null) and usage details via the injected logger.
        var rawResponse = chatMessageContent?.Content;
        _logger.LogInformation("Agent Response: {Response}", rawResponse ?? "<null>");

        // Safely deserialize the agent response. If empty or invalid JSON is returned,
        // log the problem and return an empty CompletenessResult.
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            _logger.LogWarning("Agent returned an empty response; returning empty CompletenessResult.");
            return new CompletenessResult(Array.Empty<string>(), Array.Empty<string>());
        }

        try
        {
            var result = JsonSerializer.Deserialize<CompletenessResult>(
                rawResponse,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            return result ?? new CompletenessResult(Array.Empty<string>(), Array.Empty<string>());
        }
        catch (JsonException je)
        {
            _logger.LogError(je, "Failed to parse agent JSON response. Returning empty CompletenessResult. RawResponse: {Raw}", rawResponse);
            return new CompletenessResult(Array.Empty<string>(), Array.Empty<string>());
        }
    }
}