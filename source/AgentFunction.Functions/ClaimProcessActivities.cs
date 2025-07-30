using System.Text.Json;
using AgentFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.Functions;

public class ClaimProcessActivities([FromKeyedServices("ClaimsProcessingAgent")] ChatCompletionAgent chatCompletionAgent)
{
    [Function(nameof(IsClaimComplete))]
    public async Task<bool> IsClaimComplete(
        [ActivityTrigger] Claim claim,
        FunctionContext executionContext
        )
    {
        // TODO: Add Semantic Kernel logic to check if the claim is complete
        ILogger logger = executionContext.GetLogger(nameof(IsClaimComplete));

        logger.LogInformation("Checking if claim {claimId} is complete.", claim.ClaimId);

        // var modelId = Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL_ID");
        // var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        // var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        // var kernelBuilder = Kernel.CreateBuilder()
        //                            .AddAzureOpenAIChatCompletion(deploymentName: "gpt-4o-mini",
        //                                                          modelId: "gpt-4o-mini",
        //                                                          serviceId: "gpt-4o-mini");

        // TODO: Use Azure Entra ID authentication instead of API key in production
        // var kernelBuilder = Kernel.CreateBuilder()
        //                           .AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
        // kernelBuilder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        // Kernel kernel = kernelBuilder.Build();

        // var client = kernel.GetRequiredService<IChatClient>();

        // var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // kernel.Plugins.AddFromType<ClaimsProcessingPlugin>("ClaimsProcessingPlugin");

        // Enable planning
        // OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        // {
        //     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        //  };

        // ChatCompletionAgent agent = new()
        // {
        //     Kernel = kernel,
        //     Name = "ClaimsCompletenessAgent",
        //     Description = "An agent that validates the completeness of insurance claims.",
        //     Instructions = """
        //                    You are an agent that processes insurance claims. You will validate the completeness of claims.
        //                    If the claim is complete, return true. If not, return false.                          
        //                    """,
        //     Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
        //  };

        // Deserialize the claim to a string for processing
        var claimString = JsonSerializer.Serialize(claim);
        var userMessage = "Validate the completeness of this claim: " + claimString;

        AgentResponseItem<ChatMessageContent>? responseItem = null;
        // await foreach (var item in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userMessage)))
        await foreach (var item in chatCompletionAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userMessage)))
        {
            responseItem = item;
            break; // Only need the first response
        }


        var usage = responseItem?.Message?.Metadata?["Usage"] as Microsoft.Extensions.AI.UsageDetails;
        ShowUsageDetails(usage, logger);


        // logger.LogInformation("Input tokens: {InputTokenCount}, Output tokens: {OutputTokenCount}", usage?.InputTokenCount, usage?.OutputTokenCount);
        // responseItem.Message.Metadata.Values.ToList().ForEach(m => logger.LogInformation("Metadata: {metadata}", m));
        logger.LogInformation("Agent response: {response}", responseItem?.Message.Content);

        // Simulate checking claim completeness
        await Task.Delay(1000); // Simulate async operation

        return await Task.FromResult(true);
    }

    [Function(nameof(GetClaimHistory))]
    public async Task<string> GetClaimHistory([ActivityTrigger] string claimId, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GetClaimHistory));

        logger.LogInformation("Retrieving claim history for claim {claimId}.", claimId);

        // TODO: Call MCP / API to get claim history
        // This is a placeholder implementation. Replace with actual API call.

        // Simulate retrieving claim history
        var history = await Task.FromResult("Claim history data");

        return history;
    }


    [Function(nameof(IsClaimFraudulent))]
    public async Task<bool> IsClaimFraudulent([ActivityTrigger] object input, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(IsClaimFraudulent));

        logger.LogInformation("Checking if claim is fraudulent with input: {input}.", input);

        // TODO: Add Semantic Kernel logic to check if the claim is fraudulent
        // This is a placeholder implementation. Replace with actual fraud detection logic.
        await Task.Delay(1000); // Simulate async operation

        bool isFraudulent = false; // Replace with actual fraud detection logic

        return await Task.FromResult(isFraudulent);
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
            logger.LogInformation("Input tokens: {InputTokenCount}, Output tokens: {OutputTokenCount}, Total tokens: {TotalTokenCount}", usage.InputTokenCount, usage.OutputTokenCount, usage.TotalTokenCount);

            ShowAdditionalProperties(usage.AdditionalCounts, logger);
        }
    }

    private static void ShowAdditionalProperties(AdditionalPropertiesDictionary<long> additionalCounts, ILogger logger)
    {
        if (additionalCounts is not null && additionalCounts.Count > 0)
        {
            foreach (var kvp in additionalCounts)
            {
                logger.LogInformation("Additional count - {Key}: {Value}", kvp.Key, kvp.Value);
            }
        }
    }
}
