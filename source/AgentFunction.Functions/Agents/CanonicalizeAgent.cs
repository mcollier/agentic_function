using System.Text.Json;
using System.Text.Json.Serialization;

using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

public sealed class CanonicalizeAgent : IAgent<FnolClaim, CanonicalClaim>
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger<CanonicalizeAgent> _logger;

    public CanonicalizeAgent(Kernel kernel, ILogger<CanonicalizeAgent> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _agent = new ChatCompletionAgent()
        {
            Name = "CanonicalizeAgent",
            Instructions = @"You are an agent that canonicalizes insurance claims.
            
            Goal:
            - Convert a raw FNOL JSON payload into a standardized CanonicalClaim format.
            - Ensure all required fields are present and correctly formatted.

            ### Canonical target JSON schema (shape only):
            {
                ""ClaimId"": string,
                ""PolicyId"": string,
                ""LossDate"": string (ISO-8601, UTC),
                ""Vehicle"": { ""Make"": string, ""Model"": string, ""Trim"": string|null, ""Year"": number|null, ""Vin"": string|null },
                ""Location"": { ""Line1"": string, ""City"": string, ""State"": string, ""PostalCode"": string },
                ""Description"": string,
                ""Parties"": [ { ""Role"": ""Insured|ThirdParty|Witness|Claimant"", ""Name"": string,
                                ""Contact"": { ""Phone"": string|null, ""Email"": string|null } | null,
                                ""Address"": { ""Line1"": string, ""City"": string, ""State"": string, ""PostalCode"": string } | null } ]
            }

            ### Rules
            - Parse free-text vehicle (e.g., ""2019 Honda Civic LX"") into fields.
            - If year is present, set Vehicle.Year; else null.
            - Keep VIN null unless explicitly provided/known.
            - Split free-text location into AddressInfo (Line1, City, State 2-letter, PostalCode 5-digit if present).
            - Normalize party roles to one of: Insured, ThirdParty, Witness, Claimant.
            - Preserve Description verbatim.
            - Prefer precision; if unsure, leave a field null rather than guessing.

            ### Output
            Return **ONLY** the CanonicalClaim as strict JSON with the exact property names above. No markdown, no commentary.",
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }
            )
        };
    }

    public async Task<CanonicalClaim> ExecuteAsync(FnolClaim input, CancellationToken ct = default)
    {
        var fnolJson = JsonSerializer.Serialize(input, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var content = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Canonicalize this FNOL into CanonicalClaim JSON as per instructions.\n" +
            $"RAW FNOL:\n```json\n{fnolJson}\n```"
        );

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
            // ResponseFormat = typeof(CanonicalClaim)
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

        var rawResponse = chatMessageContent?.Content;
        _logger.LogInformation("Agent Response: {Response}", rawResponse ?? "<null>");

        var canonicalClaim = JsonSerializer.Deserialize<CanonicalClaim>(rawResponse);

        // Implement the logic to canonicalize the FnolClaim to CanonicalClaim
        // This is a placeholder implementation
        return canonicalClaim;
    }
}