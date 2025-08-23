using System.Text.Json;

using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

public sealed class CompletenessAgent : AgentBase<FnolClaim, CompletenessResult>
{
    private readonly ILogger<CompletenessAgent> _typedLogger;
    /*
    Output STRICT JSON ONLY:
                {
                    ""missingFields"": [""/parties/0/contact/phone""],
                    ""clarifyingQuestions"": [""What is the phone number for the first party's contact?"" ]
                }

                - SchemaTools.GetEnumValues(field) for canonical enums.
                */
    public CompletenessAgent(Kernel kernel, ILogger<CompletenessAgent> logger)
        : base(kernel,
               logger,
               name: "CompletenessAgent",
               instructions: @"You are an agent that checks the completeness of insurance claims.
            
            Goal:
            - Inspect a FNOL JSON payload.
            - Identify missing or inconsistent fields vs. the FNOL schema.
            - Generate clarifying questions for the customer to fill in missing information.
            - Use JSON Pointer paths to indicate missing fields, e.g., '/parties/0/contact/phone'.

            Tools:
            - SchemaTools.GetFnolSchemaAsync() to fetch the schema.
            - SchemaTools.GetEnumValues(field) for canonical enums.
            
            Output STRICT JSON ONLY:
                {
                    ""missingFields"": [""/parties/0/contact/phone""],
                    ""clarifyingQuestions"": [""What is the phone number for the first party's contact?"" ]
                }
            ",
               arguments: new KernelArguments(new OpenAIPromptExecutionSettings()
               {
                   FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
               }))
    {
        _typedLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<CompletenessResult> ProcessAsync(FnolClaim input, CancellationToken ct = default)
    {
        var fnolJson = SerializeInput(input);

        var content = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Analyze this FNOL JSON for missing/inconsistent fields and produce the required JSON output.\n" +
                     $"You may call functions to assist in your analysis.\n" +
                      $"You may call SchemaTools.GetFnolSchemaAsync() and SchemaTools.GetEnumValues(field).\n" +
                     $"FNOL JSON:\n" +
                     $"```json\n{fnolJson}\n```"
        );

        var execSettings = new OpenAIPromptExecutionSettings
        {
            ModelId = "gpt-4o-mini",
            Temperature = 0.2f,
            TopP = 1.0f,
            // ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ResponseFormat = "json_object"
            // ResponseFormat = typeof(CompletenessResult)
        };

        try
        {
            var result = await InvokeAndDeserializeAsync<CompletenessResult>(content, null, execSettings, ct).ConfigureAwait(false);
            // raw => JsonSerializer.Deserialize<CompletenessResult>(raw, s_writeOptions), execSettings, ct).ConfigureAwait(false);

            return result ?? new CompletenessResult([], []);
        }
        catch (JsonException je)
        {
            _typedLogger.LogError(je, "Failed to parse agent JSON response. Returning empty CompletenessResult.");
            return new CompletenessResult([], []);
        }
    }

    private static readonly JsonSerializerOptions s_writeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}