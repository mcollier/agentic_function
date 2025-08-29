using System.Text.Json;

using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AgentFunction.Functions.Agents;

public sealed class CommsAgent(Kernel kernel, ILogger<CommsAgent> logger) :
                    AgentBase<ClaimAnalysisReport, CommsResult>(
                        kernel,
                        logger,
                        name: "CommsAgent",
                        instructions: """
                                You are an AI assistant that generates customer communications based on a auto insurance claim analysis report.
                                
                                ## GOAL
                                Generate clear, empathetic, and compliant customer-facing messages based on the claim’s status and analysis.
                                Produce variants for email, SMS, and in-app.

                                ## INPUTS
                                - ClaimAnalysisReport: includes FnolClaim, CompletenessResult, CanonicalClaim, CoverageResult, FraudResult.

                                Given a claim analysis report, generate:
                                1) An email to the customer with subject, body, recipient email address (the insured party's email), and recipient name (the insured party's name).
                                2) A concise SMS message.
                                
                                ## OUTPUT (STRICT JSON, no markdown)
                                    {
                                        ""email"": { ""subject"": ""string"", ""body"": ""valid HTML body"", ""recipientEmailAddress"": ""string"", ""recipientName"": ""string"" },
                                        ""sms"": ""string <= 320 chars""
                                    }
                            
                                ## TONE and STYLE
                                - Friendly, concise, 8th-grade reading level.
                                - Avoid jargon; explain next steps plainly.
                                - Respect privacy; do not include sensitive data in SMS.
                                - For email HTML, use simple tags (<p>, <ul>, <li>, <strong>).
                                - Use a bulleted list for important details.
                                - Use bold <strong> tags for emphasis of important items.
                                - If a field is missing, omit that section rather than guessing.

                                ## POLICY GUARDRAILS
                                - If CoverageResult.covered == false, avoid blame; provide next steps and contact options.
                                - If CoverageResult.covered == true, acknowledge coverage and include the deductible amount (if applicable).
                                - If CompletenessResult has missingFields, list max 3 actionable items.
                                - If FraudResult.score >= 0.6, avoid the term ""fraud""; say ""additional review"" and extend timelines.
                                - Never commit to payout amounts.
                                - Always include a support path (1-800-555-5555 and https://www.censurance.com).

                            """
                            )
{
    private readonly ILogger<CommsAgent> _typedLogger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<CommsResult> ProcessAsync(ClaimAnalysisReport input, CancellationToken ct = default)
    {
        string reportJson = SerializeInput(input);

        var userMessage = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Generate customer communications based on this claim analysis report.\n" +
                     $"Return STRICT JSON with keys: email, sms.\n" +
                     $"Claim Analysis Report JSON:\n" +
                     $"```json\n{reportJson}\n```"
        );

        try
        {
            var azureExecSettings = new AzureOpenAIPromptExecutionSettings
            {
                ServiceId = "gpt-4.1",
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0.2f,
                TopP = 1.0f,
                ResponseFormat = "json_object"
            };

            var result = await InvokeAndDeserializeAsync<CommsResult>(
                userMessage,
                customDeserializer: null,
                execSettings: azureExecSettings,
                cancellationToken: ct).ConfigureAwait(false);

            return result ?? new CommsResult(null!, null!);
        }
        catch (JsonException ex)
        {
            _typedLogger.LogError(ex, "Failed to parse agent response JSON.");
            return new CommsResult(null!, null!);
        }
    }
}