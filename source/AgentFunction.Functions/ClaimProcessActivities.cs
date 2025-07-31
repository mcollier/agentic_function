using System.Text.Json;
using AgentFunction.Functions.Models;
using AgentFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.Functions;

public class ClaimProcessActivities([FromKeyedServices("ClaimsProcessingAgent")] ChatCompletionAgent claimsProcessingAgent)
{
    [Function(nameof(IsClaimComplete))]
    public async Task<ClaimCompletionResult> IsClaimComplete(
        [ActivityTrigger] Claim claim,
        FunctionContext executionContext
        )
    {
        ILogger logger = executionContext.GetLogger(nameof(IsClaimComplete));

        logger.LogInformation("Checking if claim {claimId} is complete.", claim.ClaimId);

        // Deserialize the claim to a string for processing
        var claimString = JsonSerializer.Serialize(claim);

        var prompt = $"Validate the completeness of the claim.\n"+
                    $"Claim details: {claimString}\n"+
                    $"Return a JSON object with the following structure:\n"+
                    $"{{\n"+
                    $"  \"ClaimId\": \"<claim id>\",\n"+
                    $"  \"IsComplete\": true/false,\n"+
                    $"  \"Message\": \"<explanation of completeness>\"\n"+
                    $"}}";

        AgentResponseItem<ChatMessageContent> ? responseItem = null;
        await foreach (var item in claimsProcessingAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, prompt)))
        {
            responseItem = item;
            break; // Only need the first response
        }

        var usage = responseItem?.Message?.Metadata?["Usage"] as UsageDetails;
        ShowUsageDetails(usage, logger);

        logger.LogInformation("Agent response: {response}", responseItem?.Message.Content);

        if (responseItem?.Message?.Content is null)
        {
            logger.LogWarning("Agent response content was null. Returning incomplete result.");
            return await Task.FromResult(new ClaimCompletionResult(
                claim.ClaimId,
                false,
                "Agent response was null or empty."
            ));
        }

        ClaimCompletionResult? result = JsonSerializer.Deserialize<ClaimCompletionResult>(responseItem.Message.Content);

        if (result is null)
        {
            logger.LogWarning("Deserialization of agent response failed. Returning incomplete result.");
            return await Task.FromResult(new ClaimCompletionResult(
                claim.ClaimId,
                false,
                "Failed to deserialize agent response."
            ));
        }

        return await Task.FromResult(result);
    }

    [Function(nameof(GetClaimHistory))]
    public async Task<ClaimHistory> GetClaimHistory([ActivityTrigger] string claimId, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GetClaimHistory));

        logger.LogInformation("Retrieving claim history for claim {claimId}.", claimId);

        // TODO: Call MCP / API to get claim history
        // This is a placeholder implementation. Replace with actual API call.

        var claimHistory = new ClaimHistory();
            
        // Simulate retrieving claim history
        var history = await Task.FromResult("Claim history data");

        return claimHistory;
    }


    [Function(nameof(IsClaimFraudulent))]
    public async Task<ClaimFraudResult> IsClaimFraudulent([ActivityTrigger] ClaimFraudRequest claimFraudRequest, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(IsClaimFraudulent));

        logger.LogInformation("Checking if claim is fraudulent with input: {input}.", claimFraudRequest);

        var prompt = $"Analyze the following claim and its history to determine if it is potentially fraudulent.\n" +
                     $"Claim details: {JsonSerializer.Serialize(claimFraudRequest.Claim)}\n" +
                     $"Claim history: {JsonSerializer.Serialize(claimFraudRequest.History)}\n" +
                     $"Return a JSON object with the following structure:\n" +
                     $"{{\n" +
                     $"  \"ClaimId\": \"<claim id>\",\n"+
                     $"  \"IsFraudulent\": true/false,\n" +
                     $"  \"Reason\": \"<explanation of fraud detection>\"\n" +
                     $"  \"Confidence\": 0-100\n" +
                     $"}}";

        AgentResponseItem<ChatMessageContent> ? responseItem = null;
        await foreach (var item in claimsProcessingAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, prompt)))
        {
            responseItem = item;
            break; // Only need the first response
        }

        var usage = responseItem?.Message?.Metadata?["Usage"] as UsageDetails;
        ShowUsageDetails(usage, logger);

        logger.LogInformation("Agent response: {response}", responseItem?.Message.Content);

        ClaimFraudResult? fraudResult = JsonSerializer.Deserialize<ClaimFraudResult>(responseItem?.Message.Content);

        if (fraudResult is null)
        {
            logger.LogWarning("Deserialization of agent response failed. Returning false.");
            return await Task.FromResult(new ClaimFraudResult());
        }

        return await Task.FromResult(fraudResult);
    }

    [Function(nameof(DecideAction))]
    public static async Task<string> DecideAction([ActivityTrigger] object input, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(DecideAction));

        logger.LogInformation("Deciding action for claim with input: {input}.", input);

        // TODO: Add Semantic Kernel logic to decide the next action
        // This is a placeholder implementation. Replace with actual decision-making logic.
        await Task.Delay(1000); // Simulate async operation

        // Simulate decision-making logic
        string decision = "escalate"; // Replace with actual decision logic

        return await Task.FromResult(decision);
    }

    [Function(nameof(GenerateClaimSummary))]
    public static async Task<string> GenerateClaimSummary([ActivityTrigger] Claim claim, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GenerateClaimSummary));

        logger.LogInformation("Generating claim summary for claim {claimId}.", claim.ClaimId);

        // TODO: Add Semantic Kernel logic to generate claim summary
        // This is a placeholder implementation. Replace with actual summary generation logic.
        await Task.Delay(1000); // Simulate async operation 

        // Simulate generating a summary
        var summary = $"Summary for claim {claim.ClaimId}: {claim.AccidentDescription}";

        return await Task.FromResult(summary);
    }

    [Function(nameof(NotifyClaimant))]
    public async Task<string> NotifyClaimant([ActivityTrigger] object input, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(NotifyClaimant));
        logger.LogInformation("Notifying claimant with input: {input}.", input);

        // Simulate sending notification to claimant
        await Task.Delay(1000); // Simulate async operation

        return $"Claimant notified with input: {input}.";
    }

    [Function(nameof(NotifyAdjuster))]
    public static async Task<string> NotifyAdjuster([ActivityTrigger] Claim claim, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(NotifyAdjuster));
        logger.LogInformation("Notifying adjuster for claim {claimId}.", claim.ClaimId);

        // Simulate sending notification to adjuster
        await Task.Delay(1000); // Simulate async operation

        return $"Adjuster notified for claim {claim.ClaimId}.";
    }

    /// <summary>
    /// Logs usage details if available.
    /// </summary>
    /// <param name="usage">The usage details object.</param>
    /// <param name="logger">The logger instance.</param>
    private static void ShowUsageDetails(UsageDetails? usage, ILogger logger)
    {
        if (usage is not null)
        {
            logger.LogInformation("Input tokens: {InputTokenCount}, Output tokens: {OutputTokenCount}, Total tokens: {TotalTokenCount}",
                usage.InputTokenCount,
                usage.OutputTokenCount,
                usage.TotalTokenCount);

            if (usage.AdditionalCounts is not null && usage.AdditionalCounts.Count > 0)
            {
                foreach (var kvp in usage.AdditionalCounts)
                {
                    logger.LogInformation("Additional count - {Key}: {Value}", kvp.Key, kvp.Value);
                }
            }
        }
    }
}
