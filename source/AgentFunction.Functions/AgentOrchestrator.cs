using System.Text.Json;
using AgentFunction.Functions;
using AgentFunction.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions;

public class AgentOrchestrator
{
    // private static Claim GetClaimData(ILogger logger, string requestBody)
    // {
    //     Claim claim;
    //     if (!string.IsNullOrWhiteSpace(requestBody))
    //     {
    //         try
    //         {
    //             var deserialized = JsonSerializer.Deserialize<Claim>(requestBody);
    //             if (deserialized is not null)
    //             {
    //                 claim = deserialized;
    //             }
    //             else
    //             {
    //                 logger.LogInformation("Deserialized claim is null. Using default claim for testing.");
    //                 claim = GetDefaultClaim();
    //             }
    //         }
    //         catch (JsonException ex)
    //         {
    //             logger.LogWarning(ex, "Failed to deserialize claim data from request body. Using default claim.");
    //             claim = GetDefaultClaim();
    //         }
    //     }
    //     else
    //     {
    //         logger.LogInformation("No valid claim data in POST body. Using default claim for testing.");
    //         claim = GetDefaultClaim();
    //     }

    //     return claim;
    // }

    // private static Claim GetDefaultClaim()
    // {
    //     return new Claim
    //     {
    //         ClaimId = "12345",
    //         PolicyNumber = "POLICY-67890",
    //         ClaimantName = "John Doe",
    //         ClaimantContact = "john.doe@example.com",
    //         DateOfAccident = new DateTime(2023, 1, 1),
    //         AccidentDescription = "Minor fender bender.",
    //         VehicleMake = "Toyota",
    //         VehicleModel = "Camry",
    //         VehicleYear = 2020,
    //         LicensePlate = "ABC123",
    //         AmountClaimed = 1000.00m,
    //         Status = ClaimStatus.Submitted
    //     };
    // }

    [Function(nameof(ClaimProcessingOrchestration))]
    public async Task ClaimProcessingOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var claim = context.GetInput<Claim>()
            ?? throw new ArgumentNullException(nameof (context), "No claim data provided to orchestration.");

        ILogger logger = context.CreateReplaySafeLogger(nameof(ClaimProcessingOrchestration));
        logger.LogInformation("Starting claim processing orchestration.");


        // Step 1: Assess completeness of the claim
        var isComplete = await context.CallActivityAsync<bool>(nameof(ClaimProcessActivities.IsClaimComplete), claim);
        if (!isComplete)
        {
            logger.LogInformation("Claim is incomplete. Orchestration will not proceed.");
            return;
        }

        // Step 2: Get history from MCP plugin via SK
        var history = await context.CallActivityAsync<string>(nameof(ClaimProcessActivities.GetClaimHistory), claim.ClaimId);
        logger.LogInformation("Claim history retrieved: {history}", history);
        if (string.IsNullOrEmpty(history))
        {
            logger.LogWarning("No claim history found for claim ID {claimId}.", claim.ClaimId);
            return;
        }

        // Step 3: Fraud detection using SK
        var isFraudulent = await context.CallActivityAsync<bool>(nameof(ClaimProcessActivities.IsClaimFraudulent), new { claim, history });
        if (isFraudulent)
        {
            logger.LogWarning("Claim {claimId} is marked as fraudulent.", claim.ClaimId);
            return;
        }

        // Step 4: Decide the next step
        var decision = await context.CallActivityAsync<string>(nameof(ClaimProcessActivities.DecideAction), new { claim, isFraudulent });

        if (decision == "escalate")
        {
            await context.CallActivityAsync(nameof(ClaimProcessActivities.NotifyAdjuster), claim);
            // await context.WaitForExternalEvent<bool>("AdjusterApproval");
        }

        // Step 5: Generate summary
        var summary = await context.CallActivityAsync<string>(nameof(ClaimProcessActivities.GenerateClaimSummary), claim);

        // Step 6: Notify the claimant
        var notificationResult = await context.CallActivityAsync<string>(nameof(ClaimProcessActivities.NotifyClaimant), new { Email = claim.ClaimantContact, Body = summary });

        logger.LogInformation("Completed claim processing orchestration.");
    }   
}