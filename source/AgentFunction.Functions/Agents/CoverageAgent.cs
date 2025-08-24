using System.Text.Json;

using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

/**
- Use only the provided policy text.
- If the claim is outside the policy limits, respond with 'no'.

            Tools:
            - PolicyTools.GetPolicyDetailsByIdAsync(policyId) to fetch policy details by policy ID.

             ## Tools:
            - PolicyTools.GetPolicyDetailsByIdAsync(policyId) to fetch policy details by policy ID.

            - Only call get_policy_details_by_id if you have a valid policyId (e.g., P-998877). Call at most once per policy and reuse the retrieved text for all reasoning/citations.

            ## Rules
            - If the claim is covered by the policy, provide a brief justification.
            - If the claim is not covered by the policy, provide concise justification. Confidence must be more than 80% to determine the claim is not covered.
*/
public sealed class CoverageAgent : AgentBase<CanonicalClaim, CoverageResult>
{
    private readonly ILogger<CoverageAgent> _typedLogger;

    public CoverageAgent(Kernel kernel, ILogger<CoverageAgent> logger)
        : base(kernel,
               logger,
               name: "CoverageAgent",
               instructions: @"You are an agent that checks insurance coverage for claims.
            
            ## Goal:
            - Inspect a canonical claim JSON payload.
            - Determine if the claim is covered by the policy.
            - Provide a clear 'yes' or 'no' answer.
            - Include the deductible amount (if applicable) based on the coverage.
            - Include a confidence score (0-1.0) indicating your certainty.

            ## Target JSON schema (shape only)
            {
                ""type"": ""object"",
                ""properties"": {
                    ""Confidence"": { ""type"": ""number"" },
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
                ""required"": [""Covered"", ""Basis"", ""Notes"", ""Deductible"", ""CoverageLimit"", ""Confidence""]
            }

            ### Output
            Return **ONLY** the CoverageResult as strict JSON with exact property names. No markdown, no commentary.
           "
            //    arguments: new KernelArguments(new OpenAIPromptExecutionSettings()
            //    {
            //        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            //    })
               )
    {
        _typedLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<CoverageResult> ProcessAsync(CanonicalClaim input, CancellationToken ct = default)
    {
        var claimJson = SerializeInput(input);

        // string policyText = """
        //                     Collision Coverage §2.1
        //                     We cover direct and accidental loss to your covered auto caused by collision...
        //                     Deductible §2.4
        //                     A $500 deductible applies to each collision claim...
        //                     Exclusions §3.2
        //                     We do not cover losses that occur during commercial use...
        //                     """; // This should be provided or fetched from a relevant source

        var userMessage = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Analyze this claim JSON to determine if the claim is covered by the policy.\n" +
                     $"Look up the policy details using the policy ID." +
                     //  $"You may call PolicyTools.GetPolicyDetailsByIdAsync(policyId) to fetch policy details.\n" +
                     //  $"Policy text: {policyText}\n" +
                     $"Claim JSON: {claimJson}\n"
        );

        var execSettings = new AzureOpenAIPromptExecutionSettings
        {
            ServiceId = "gpt-4o-mini",
            Temperature = 0.2f,
            TopP = 1.0f,
            // ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            // ResponseFormat = typeof(CoverageResult)
            ResponseFormat = "json_object"
        };

        var coverage = await InvokeAndDeserializeAsync<CoverageResult>(
            userMessage,
            customDeserializer: null,
            execSettings: execSettings,
            cancellationToken: ct).ConfigureAwait(false);

        return coverage ?? new CoverageResult(0, false, [], "");
    }

    // private static readonly JsonSerializerOptions s_writeOptions = new()
    // {
    //     PropertyNameCaseInsensitive = true
    // };
}