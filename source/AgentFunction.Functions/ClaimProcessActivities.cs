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
using OpenAI.Responses;

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

        var cleanResult = CleanJsonResponse(responseItem.Message.Content, logger);

        ClaimCompletionResult? result = JsonSerializer.Deserialize<ClaimCompletionResult>(cleanResult);

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
    public async Task<ClaimHistoryResult> GetClaimHistory([ActivityTrigger] string customerId, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GetClaimHistory));

        logger.LogInformation("Retrieving claim history for customer {customerId}.", customerId);

        // TODO: Call MCP / API to get claim history
        // This is a placeholder implementation. Replace with actual API call.

        var prompt = $"Provide a summation of the claim history for customer {customerId}.\n" +
                     $"Return a JSON object with the following structure:\n" +
                     $"{{\n" +
                     $"  \"CustomerId\": \"{customerId}\",\n" +
                     $"  \"TotalClaims\": \"<total number of claims as an integer value>\",\n" +
                     $"  \"TotalClaimAmount\": \"<total claim amount in dollars as a decimal value>\",\n" +
                     $"  \"MostRecentClaimDate\": \"<date of most recent claim>\"\n" +
                     $"}}";

        AgentResponseItem<ChatMessageContent> ? responseItem = null;
        await foreach (var result in claimsProcessingAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, prompt)))
        {
            // Process each result
            responseItem = result;
            break; // Only need the first response
        }

        logger.LogInformation("Agent response: {response}", responseItem?.Message.Content);

        var cleanResult = CleanJsonResponse(responseItem?.Message.Content, logger);
        logger.LogInformation("Cleaned agent response: {response}", cleanResult);

        var claimHistoryResult = JsonSerializer.Deserialize<ClaimHistoryResult>(cleanResult);

        return claimHistoryResult;
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

        var cleanResult = CleanJsonResponse(responseItem?.Message.Content, logger);
        logger.LogInformation("Cleaned agent response: {response}", cleanResult);

        ClaimFraudResult? fraudResult = JsonSerializer.Deserialize<ClaimFraudResult>(cleanResult);

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
    /// Cleans a JSON response string by trimming whitespace and extracting the JSON object if needed.
    /// </summary>
    /// <param name="response">The response string to clean.</param>
    /// <returns>A cleaned JSON string.</returns>
    private static string CleanJsonResponse(string response, ILogger logger)
    {
        // Implementation from https://github.com/Azure-Samples/Durable-Task-Scheduler/blob/3eb15a20daa5126943e60adf99c0e3f1f1764a5a/samples/durable-task-sdks/dotnet/Agents/PromptChaining/Worker/Services/BaseAgentService.cs#L131

        if (string.IsNullOrEmpty(response))
        {
            logger.LogWarning("[JSON-PARSER] Response was null or empty");
            return "{}";
        }

        logger.LogInformation($"[JSON-PARSER] Processing response ({response.Length} chars)");

        // Trim any whitespace
        response = response.Trim();

        // Simple case: Check if response is already valid JSON
        try
        {
            using (JsonDocument.Parse(response))
            {
                logger.LogInformation("[JSON-PARSER] Response is valid JSON");
                return response;
            }
        }
        catch (JsonException)
        {
            logger.LogInformation("[JSON-PARSER] Initial JSON validation failed, attempting to extract JSON");
        }

        // Handle markdown code blocks if present
        if (response.Contains("```"))
        {
            // Find start and end of code block
            int codeBlockStart = response.IndexOf("```");
            int codeBlockEnd = response.LastIndexOf("```");

            if (codeBlockStart != codeBlockEnd) // Make sure we found both opening and closing markers
            {
                // Extract content between code blocks
                int startIndex = response.IndexOf('\n', codeBlockStart) + 1;
                int endIndex = codeBlockEnd;
                
                // Make sure we have valid start and end indices
                if (startIndex > 0 && endIndex > startIndex)
                {
                    string codeContent = response.Substring(startIndex, endIndex - startIndex).Trim();
                    logger.LogInformation("[JSON-PARSER] Extracted content from code block");
                    
                    // Remove any language specifier like ```json
                    if (codeContent.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                    {
                        codeContent = codeContent.Substring(4).Trim();
                    }
                    
                    response = codeContent;
                }
            }
        }

        // Check if response is wrapped in backticks
        if (response.StartsWith("`") && response.EndsWith("`"))
        {
            response = response.Substring(1, response.Length - 2).Trim();
            logger.LogInformation("[JSON-PARSER] Removed backticks");
        }

        // Final validation
        try
        {
            using (JsonDocument.Parse(response))
            {
                logger.LogInformation("[JSON-PARSER] Successfully validated JSON");
                return response;
            }
        }
        catch (JsonException ex)
        {
            logger.LogError($"[JSON-PARSER] Failed to parse JSON: {ex.Message}");
            return "{}"; // Return empty JSON object as fallback
        }
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
