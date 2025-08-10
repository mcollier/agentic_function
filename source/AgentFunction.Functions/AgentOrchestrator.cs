using AgentFunction.Models;

using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions;

public class AgentOrchestrator
{
    [Function(nameof(ClaimProcessingOrchestration))]
    public async Task ClaimProcessingOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var claim = context.GetInput<Claim>()
            ?? throw new ArgumentNullException(nameof(context), "No claim data provided to orchestration.");

        ILogger logger = context.CreateReplaySafeLogger(nameof(ClaimProcessingOrchestration));
        logger.LogInformation("Starting claim processing orchestration.");

        context.SetCustomStatus(new
        {
            step = "Starting claim processing",
            message = $"Processing claim {claim.ClaimDetail.ClaimId}.",
            progress = 0
        });

        context.SetCustomStatus(new
        {
            step = "Assessing claim completeness",
            message = $"Checking if claim {claim.ClaimDetail.ClaimId} is complete.",
            progress = 10
        });

        claim.ClaimDetail.Status = ClaimStatus.UnderReview;

        // Step 1: Assess completeness of the claim
        var claimCompletionResult = await context.CallActivityAsync<ClaimCompletionResult>(nameof(ClaimProcessActivities.IsClaimComplete), claim);
        if (!claimCompletionResult.IsComplete)
        {
            logger.LogInformation("Claim is incomplete. Orchestration will not proceed.");

            // TODO: Handle incomplete claim scenario, e.g., notify claimant or log.
            
            return;
        }

        context.SetCustomStatus(new
        {
            step = "Retrieving claim history",
            message = $"Getting history for claim {claim.ClaimDetail.ClaimId}.",
            progress = 30
        });

        // Step 2: Get history from MCP plugin via SK
        var customerID = claim.Customer.CustomerId;
        var history = await context.CallActivityAsync<ClaimHistoryResult>(nameof(ClaimProcessActivities.GetClaimHistory), customerID);
        logger.LogInformation("Claim history for customer {customerId} retrieved: {history}", customerID, history);

        context.SetCustomStatus(new
        {
            step = "Detecting fraud",
            message = $"Analyzing claim {claim.ClaimDetail.ClaimId} for potential fraud.",
            progress = 60
        });

        ClaimFraudRequest claimFraudRequest = new(Claim: claim, History: history);

        // Step 3: Fraud detection using SK
        var isFraudulent = await context.CallActivityAsync<ClaimFraudResult>(nameof(ClaimProcessActivities.IsClaimFraudulent), claimFraudRequest);
        if (isFraudulent.IsFraudulent)
        {
            claim.ClaimDetail.Status = ClaimStatus.Rejected;

            logger.LogWarning("Claim {claimId} is marked as fraudulent.", claim.ClaimDetail.ClaimId);
            return;
        }

        context.SetCustomStatus(new
        {
            step = "Deciding next action",
            message = $"Deciding next action for claim {claim.ClaimDetail.ClaimId}.",
            progress = 70
        });

        // Step 4: Decide the next step
        var decision = await context.CallActivityAsync<string>(nameof(ClaimProcessActivities.DecideAction), new { claim, isFraudulent });

        if (decision == "escalate")
        {
            await context.CallActivityAsync(nameof(ClaimProcessActivities.NotifyAdjuster), claim);
            await context.WaitForExternalEvent<bool>("AdjusterApproval");
        }

        context.SetCustomStatus(new
        {
            step = "Generating claim summary",
            message = $"Creating summary for claim {claim.ClaimDetail.ClaimId}.",
            progress = 80
        });

        // If the claim is not fraudulent, approve the claim.
        claim.ClaimDetail.Status = ClaimStatus.Approved;

        // Step 5: Generate summary
        var summary = await context.CallActivityAsync<ClaimSummaryResult>(nameof(ClaimProcessActivities.GenerateClaimSummary), claim);

        context.SetCustomStatus(new
        {
            step = "Notifying claimant",
            message = $"Notifying claimant for claim {claim.ClaimDetail.ClaimId}.",
            progress = 90
        });


        // Step 6: Notify the claimant
        if (claim.Customer.ContactInfo != null)
        {
            string emailAddress = claim.Customer.ContactInfo;
            NotificationRequest notificationRequest = new(claim.ClaimDetail.ClaimId, claim.Customer.ContactInfo, summary.SummaryHtml);
            var notificationResult = await context.CallActivityAsync<string>(nameof(ClaimProcessActivities.NotifyClaimant), notificationRequest);
        }
        
        context.SetCustomStatus(new
        {
            step = "Complete",
            message = $"Claim processing for {claim.ClaimDetail.ClaimId} is complete.",
            progress = 100
        });


        logger.LogInformation("Completed claim processing orchestration.");
    }
}
