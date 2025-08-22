using System.Text.Json;
using System.Text.Json.Serialization;
using AgentFunction.Functions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

public sealed class CoverageAgent : IAgent<CanonicalClaim, CoverageResult>
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger<CoverageAgent> _logger;

    public CoverageAgent(Kernel kernel, ILogger<CoverageAgent> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _agent = new ChatCompletionAgent()
        {
            Name = "CoverageAgent",
            Instructions = @"You are an agent that checks insurance coverage for claims.
            
            Goal:
            - Inspect a canonical claim JSON payload.
            - Determine if the claim is covered by the policy.
            - Provide a clear 'yes' or 'no' answer.

            ### Rules
            - Use only the provided policy text.
            - If the claim is outside the policy limits, respond with 'no'.

            ### Target JSON schema (shape only)
            {
                ""type"": ""object"",
                ""properties"": {
                    ""Covered"": { ""type"": ""boolean"" },
                    ""Basis"": {
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""object"",
                        ""properties"": {
                        ""Section"": { ""type"": ""string"" },
                        ""Reason"": { ""type"": ""string"" }
                        },
                        ""required"": [""Section"", ""Reason""]
                    }
                    },
                    ""Notes"": { ""type"": ""string"" },
                    ""Deductible"": { ""type"": [""number"", ""null""] },
                    ""CoverageLimit"": { ""type"": [""number"", ""null""] }
                },
                ""required"": [""Covered"", ""Basis"", ""Notes""]
            }

            ### Output
            Return **ONLY** the CoverageResult as strict JSON with exact property names. No markdown, no commentary.
           ",
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }
            )
        };
    }

    public async Task<CoverageResult> ExecuteAsync(CanonicalClaim input, CancellationToken ct = default)
    {
        var claimJson = JsonSerializer.Serialize(input, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        string policyText = """
                            Collision Coverage §2.1
                            We cover direct and accidental loss to your covered auto caused by collision...
                            Deductible §2.4
                            A $500 deductible applies to each collision claim...
                            Exclusions §3.2
                            We do not cover losses that occur during commercial use...
                            """; // This should be provided or fetched from a relevant source

        var content = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Analyze this claim JSON to determine if the claim is covered by the policy.\n" +
            $"Policy text: {policyText}\n" +
            $"Claim JSON: {claimJson}\n"
        );

        var execSettings = new OpenAIPromptExecutionSettings
        {
            ModelId = "gpt-4o-mini",
            Temperature = 0.2f,
            TopP = 1.0f,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            // FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            // ResponseFormat = "json_object"
            ResponseFormat = typeof(CoverageResult)
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

        // Deserialize the response content to get the result
        var coverageResult = JsonSerializer.Deserialize<CoverageResult>(rawResponse);

        // Deserialize the result into CoverageResult
        return coverageResult;
    }
}