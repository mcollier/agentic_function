using System.Net.Http.Json;
using System.Text.Json;
using AgentFunction.Models;
using Azure.Communication.Email;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.Functions;

public class ClaimProcessActivities(IHttpClientFactory httpClientFactory, QueueServiceClient queueServiceClient)
{
    [Function(nameof(IsClaimComplete))]
    public async Task<ClaimCompletionResult> IsClaimComplete(
        [ActivityTrigger] Claim claim,
        FunctionContext executionContext
        )
    {
        ILogger logger = executionContext.GetLogger(nameof(IsClaimComplete));

        logger.LogInformation("Checking if claim {claimId} is complete.", claim.ClaimDetail.ClaimId);

        // Deserialize the claim to a string for processing
        var claimString = JsonSerializer.Serialize(claim);

        var prompt = $"Validate the completeness of the claim.\n" +
                    $"Claim details: {claimString}\n" +
                    $"Return a JSON object with the following structure:\n" +
                    $"{{\n" +
                    $"  \"ClaimId\": \"<claim id>\",\n" +
                    $"  \"IsComplete\": true/false,\n" +
                    $"  \"Message\": \"<explanation of completeness>\"\n" +
                    $"}}";

        string agentResponse = await SubmitAgentRequestAsync(prompt, logger);

        ClaimCompletionResult? claimCompletionResult = JsonSerializer.Deserialize<ClaimCompletionResult>(agentResponse);

        if (claimCompletionResult is null)
        {
            logger.LogWarning("Deserialization of agent response failed. Returning incomplete result.");
            return await Task.FromResult(new ClaimCompletionResult(
                claim.ClaimDetail.ClaimId,
                false,
                "Failed to deserialize agent response."
            ));
        }

        return await Task.FromResult(claimCompletionResult);
    }

    [Function(nameof(GetClaimHistory))]
    public async Task<ClaimHistoryResult> GetClaimHistory([ActivityTrigger] string customerId, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GetClaimHistory));

        logger.LogInformation("Retrieving claim history for customer {customerId}.", customerId);

        // Agent API should call the MCP server to get the history.

        var prompt = $"Provide a summation of the claim history for customer {customerId}.\n" +
                     $"Return a JSON object with the following structure:\n" +
                     $"{{\n" +
                     $"  \"CustomerId\": \"{customerId}\",\n" +
                     $"  \"TotalClaims\": \"<total number of claims as an integer value>\",\n" +
                     $"  \"TotalClaimAmount\": \"<total claim amount in dollars as a decimal value>\",\n" +
                     $"  \"MostRecentClaimDate\": \"<date of most recent claim>\"\n" +
                     $"}}";

        string agentResponse = await SubmitAgentRequestAsync(prompt, logger);

        ClaimHistoryResult? claimHistoryResult = JsonSerializer.Deserialize<ClaimHistoryResult>(agentResponse);

        if (claimHistoryResult is null)
        {
            logger.LogWarning("Deserialization of agent response failed. Returning default ClaimHistoryResult.");
            return await Task.FromResult(new ClaimHistoryResult
            {
                CustomerId = customerId,
                TotalClaims = 0,
                TotalClaimAmount = 0.0m,
                MostRecentClaimDate = DateTime.MinValue
            });
        }

        return await Task.FromResult(claimHistoryResult);
    }

    [Function(nameof(IsClaimFraudulent))]
    public async Task<ClaimFraudResult> IsClaimFraudulent([ActivityTrigger] ClaimFraudRequest claimFraudRequest, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(IsClaimFraudulent));

        logger.LogInformation("Checking if claim is fraudulent with input: {input}.", claimFraudRequest);

        var prompt = $"Analyze the following claim and its history to determine if it is potentially fraudulent.\n" +
                     $"Claim details: {JsonSerializer.Serialize(claimFraudRequest.Claim)}\n" +
                     $"Claim history: {JsonSerializer.Serialize(claimFraudRequest.History)}\n" +
                     $"Use the is_claim_fraudulent plugin to determine if the claim is fraudulent.\n" +
                     $"Return a JSON object with the following structure:\n" +
                     $"{{\n" +
                     $"  \"ClaimId\": \"<claim id>\",\n" +
                     $"  \"IsFraudulent\": true/false,\n" +
                     $"  \"Reason\": \"<explanation of fraud detection>\"\n" +
                     $"  \"Confidence\": 0-100\n" +
                     $"}}";

        string agentResponse = await SubmitAgentRequestAsync(prompt, logger);
        ClaimFraudResult? fraudResult = JsonSerializer.Deserialize<ClaimFraudResult>(agentResponse);
        if (fraudResult is null)
        {
            logger.LogWarning("Deserialization of agent response failed. Returning default ClaimFraudResult.");
            return await Task.FromResult(new ClaimFraudResult
            {
                ClaimId = claimFraudRequest.Claim.ClaimDetail.ClaimId,
                IsFraudulent = false,
                Reason = "Failed to deserialize agent response.",
                Confidence = 0
            });
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
    public async Task<ClaimSummaryResult> GenerateClaimSummary([ActivityTrigger] Claim claim, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GenerateClaimSummary));

        logger.LogInformation("Generating claim summary for claim {claimId}.", claim.ClaimDetail.ClaimId);

        var prompt = $"Generate a lighthearted, engaging, and claimant-friendly summary of the claim's status or outcome.\n" +
                     $"Ensure the tone is upbeat but professional and appropriate for all claimants.\n" +
                     $"Highlight key details such as the claim ID, accident description, and claim amount.\n" +
                     $"Provide a clear list of next steps or recommendations for the claimant.\n" +
                     $"Provide two options for the summary: a plain-text and an HTML version.\n" +
                     $"Claim ID: {claim.ClaimDetail.ClaimId}\n" +
                     $"Accident Description: {claim.ClaimDetail.AccidentDescription}\n" +
                     $"Claim Amount: {claim.ClaimDetail.AmountClaimed}\n" +
                     $"Return a JSON object with the following structure:\n" +
                     $"{{\n" +
                     $"  \"ClaimId\": \"{claim.ClaimDetail.ClaimId}\",\n" +
                     $"  \"Summary\": \"<summary of the claim>\",\n" +
                     $"  \"SummaryHtml\": \"<summary of the claim in HTML>\"\n" +
                     $"}}";

        string agentResponse = await SubmitAgentRequestAsync(prompt, logger);

        ClaimSummaryResult? summaryResult = JsonSerializer.Deserialize<ClaimSummaryResult>(agentResponse);
        if (summaryResult is null)
        {
            logger.LogWarning("Deserialization of agent response failed. Returning default summary.");
            return await Task.FromResult(new ClaimSummaryResult
            {
                ClaimId = claim.ClaimDetail.ClaimId,
                Summary = "Failed to generate summary due to deserialization error."
            });
        }

        // Simulate generating a summary
        var summary = $"Summary for claim {claim.ClaimDetail.ClaimId}: {summaryResult.Summary}";

        return await Task.FromResult(summaryResult);
    }

    [Function(nameof(NotifyClaimant))]
    public async Task<string> NotifyClaimant([ActivityTrigger] NotificationRequest notificationRequest, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(NotifyClaimant));
        logger.LogInformation("Notifying claimant with input: {input}.", notificationRequest);

        string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Azure Communication Service connection string is not set.");
        string senderEmailAddress = Environment.GetEnvironmentVariable("SENDER_EMAIL_ADDRESS")
            ?? throw new InvalidOperationException("Sender email address is not set.");

        string recipientEmailAddress = Environment.GetEnvironmentVariable("RECIPIENT_EMAIL_ADDRESS")
            ?? throw new InvalidOperationException("Recipient email address is not set.");

        var emailClient = new EmailClient(connectionString);
        var sender = senderEmailAddress;
        var recipient = recipientEmailAddress; //notificationRequest.EmailAddress;
        var subject = "Claim Notification";

        var emailMessage = new EmailMessage(
            senderAddress: sender,
            content: new EmailContent(subject: subject)
            {
                PlainText = notificationRequest.EmailBody,
                Html = notificationRequest.EmailBody
            },
            recipients: new EmailRecipients(new List<EmailAddress> {
                new EmailAddress(recipient)
            })
        );

        var emailSendOperation = await emailClient.SendAsync(
            wait: Azure.WaitUntil.Completed,
            senderAddress: sender,
            recipientAddress: recipient,
            subject: subject,
            htmlContent: notificationRequest.EmailBody
        );

        logger.LogInformation("Email sent for claim {claimId} to {recipient}.  Status: {status}", notificationRequest.ClaimId, recipient, emailSendOperation.Value.Status);

        return $"Claimant notified with input: {notificationRequest}.";
    }

    [Function(nameof(NotifyAdjuster))]
    public async Task<string> NotifyAdjuster([ActivityTrigger] Claim claim, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(NotifyAdjuster));
        logger.LogInformation("Notifying adjuster for claim {claimId}.", claim.ClaimDetail.ClaimId);

        // Write a message to the Azure Storage Queue
        var queueClient = queueServiceClient.GetQueueClient("claim-notifications");
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(JsonSerializer.Serialize(claim));

        return $"Adjuster notified for claim {claim.ClaimDetail.ClaimId}.";
    }

    /// <summary>
    /// Cleans a JSON response string by trimming whitespace and extracting the JSON object if needed.
    /// </summary>
    /// <param name="response">The response string to clean.</param>
    /// <returns>A cleaned JSON string.</returns>
    private static string CleanJsonResponse(string? response, ILogger logger)
    {
        // Implementation from https://github.com/Azure-Samples/Durable-Task-Scheduler/blob/3eb15a20daa5126943e60adf99c0e3f1f1764a5a/samples/durable-task-sdks/dotnet/Agents/PromptChaining/Worker/Services/BaseAgentService.cs#L131

        if (string.IsNullOrEmpty(response))
        {
            logger.LogWarning("[JSON-PARSER] Response was null or empty");
            return "{}";
        }

        logger.LogInformation("[JSON-PARSER] Processing response ({response.Length} chars)", response.Length);

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

    private async Task<string> SubmitAgentRequestAsync(string prompt, ILogger logger)
    {
        var request = new AgentCompletionRequest
        {
            Prompt = prompt,
            ChatHistory = [],
            IsStreaming = false
        };

        // Call the Semantic Kernel agent via HTTP client
        var claimsAgentClient = httpClientFactory.CreateClient("claimsagent");
        var result = await claimsAgentClient.PostAsJsonAsync("agent/completions", request);

        result.EnsureSuccessStatusCode();

        var responseContent = await result.Content.ReadFromJsonAsync<ChatMessageContent>().ConfigureAwait(false);

        logger.LogInformation("Agent response: {response}", responseContent?.Content);

        var cleanResult = CleanJsonResponse(responseContent?.Content, logger);

        return cleanResult;
    }
}

public sealed class AgentCompletionRequest
{
    /// <summary>
    /// Gets or sets the prompt.
    /// </summary>
    public required string Prompt { get; set; }

    /// <summary>
    /// Gets or sets the chat history.
    /// </summary>
    public required ChatHistory ChatHistory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether streaming is requested.
    /// </summary>
    public bool IsStreaming { get; set; }
}