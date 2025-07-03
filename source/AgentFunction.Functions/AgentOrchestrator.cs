using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public static class AgentOrchestrator
{
    // [Function(nameof(AgentOrchestrator))]
    // public static async Task<List<string>> RunOrchestrator(
    //     [OrchestrationTrigger] TaskOrchestrationContext context)
    // {
    //     ILogger logger = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
    //     logger.LogInformation("Saying hello.");
    //     var outputs = new List<string>
    //     {
    //         // Replace name and input with values relevant for your Durable Functions Activity
    //         await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"),
    //         await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"),
    //         await context.CallActivityAsync<string>(nameof(SayHello), "London")
    //     };

    //     // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
    //     return outputs;
    // }

    // [Function(nameof(SayHello))]
    // public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
    // {
    //     ILogger logger = executionContext.GetLogger("SayHello");
    //     logger.LogInformation("Saying hello to {name}.", name);
    //     return $"Hello {name}!";
    // }

    [Function(nameof(AgentOrchestrator_HttpStart))]
    public static async Task<HttpResponseData> AgentOrchestrator_HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(AgentOrchestrator_HttpStart));

        // Try to read claim data from the HTTP POST body
        var requestBody = await req.ReadAsStringAsync() ?? string.Empty;
        MyClaimData claim = GetClaimData(logger, requestBody);

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ClaimProcessingOrchestration), claim);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }

    private static MyClaimData GetClaimData(ILogger logger, string requestBody)
    {
        MyClaimData claim;
        if (!string.IsNullOrWhiteSpace(requestBody))
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<MyClaimData>(requestBody);
                if (deserialized is not null)
                {
                    claim = deserialized;
                }
                else
                {
                    logger.LogInformation("Deserialized claim is null. Using default claim for testing.");
                    claim = GetDefaultClaim();
                }
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Failed to deserialize claim data from request body. Using default claim.");
                claim = GetDefaultClaim();
            }
        }
        else
        {
            logger.LogInformation("No valid claim data in POST body. Using default claim for testing.");
            claim = GetDefaultClaim();
        }

        return claim;
    }

    private static MyClaimData GetDefaultClaim()
    {
        return new MyClaimData
        {
            ClaimId = "12345",
            PolicyNumber = "POLICY-67890",
            ClaimantName = "John Doe",
            ClaimantContact = "john.doe@example.com",
            DateOfAccident = new DateTime(2023, 1, 1),
            AccidentDescription = "Minor fender bender.",
            VehicleMake = "Toyota",
            VehicleModel = "Camry",
            VehicleYear = 2020,
            LicensePlate = "ABC123",
            AmountClaimed = 1000.00m,
            Status = ClaimStatus.Submitted
        };
    }

    [Function(nameof(ClaimProcessingOrchestration))]
    public static async Task ClaimProcessingOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(ClaimProcessingOrchestration));
        logger.LogInformation("Starting claim processing orchestration.");


        var claim = context.GetInput<MyClaimData>();
        if (claim is null)
        {
            logger.LogWarning("No claim data provided. Exiting orchestration.");
            return;
        }

        // Step 1: Assess completeness of the claim

        var isComplete = await context.CallActivityAsync<bool>("CallSK_IsClaimComplete", claim);
        if (!isComplete)
        {
            logger.LogInformation("Claim is incomplete. Orchestration will not proceed.");
            return;
        }

        // Step 2: Get history from MCP plugin via SK

        var history = await context.CallActivityAsync<string>("CallSK_GetClaimHistory", claim.ClaimId);
        logger.LogInformation("Claim history retrieved: {history}", history);
        if (string.IsNullOrEmpty(history))
        {
            logger.LogWarning("No claim history found for claim ID {claimId}.", claim.ClaimId);
            return;
        }

        // Step 3: Fraud detection using SK

        var isFraudulent = await context.CallActivityAsync<bool>("CallSK_IsClaimFraudulent", new { claim, history });
        if (isFraudulent)
        {
            logger.LogWarning("Claim {claimId} is marked as fraudulent.", claim.ClaimId);
            return;
        }

        // Step 4: Decide the next step

        var decision = await context.CallActivityAsync<string>("CallSK_DecideAction", new { claim, isFraudulent });

        if (decision == "escalate")
        {
            await context.CallActivityAsync("NotifyAdjuster", claim);
            // await context.WaitForExternalEvent<bool>("AdjusterApproval");
        }

        // Step 5: Generate summary
        var summary = await context.CallActivityAsync<string>("CallSK_GenerateClaimSummary", claim);

        // Step 6: Notify the claimant
        var notificationResult = await context.CallActivityAsync<string>("NotifyClaimant", new { Email = claim.ClaimantContact, Body = summary });

        logger.LogInformation("Completed claim processing orchestration.");
        // return results;
    }
    [Function(nameof(NotifyClaimant))]
    public static async Task<string> NotifyClaimant([ActivityTrigger] object input, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(NotifyClaimant));
        logger.LogInformation("Notifying claimant with input: {input}.", input);

        // Simulate sending notification to claimant
        await Task.Delay(1000); // Simulate async operation

        return $"Claimant notified with input: {input}.";
    }

    [Function(nameof(NotifyAdjuster))]
    public static async Task<string> NotifyAdjuster([ActivityTrigger] MyClaimData claim, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(NotifyAdjuster));
        logger.LogInformation("Notifying adjuster for claim {claimId}.", claim.ClaimId);

        // Simulate sending notification to adjuster
        await Task.Delay(1000); // Simulate async operation

        return $"Adjuster notified for claim {claim.ClaimId}.";
    }

    [Function(nameof(CallSK_IsClaimComplete))]
    public static async Task<bool> CallSK_IsClaimComplete([ActivityTrigger] MyClaimData claim, FunctionContext executionContext)
    {
        // TODO: Add Semantic Kernel logic to check if the claim is complete
        ILogger logger = executionContext.GetLogger(nameof(CallSK_IsClaimComplete));

        logger.LogInformation("Checking if claim {claimId} is complete.", claim.ClaimId);

        // Simulate checking claim completeness

        return await Task.FromResult(true); // Simulate checking claim completeness
    }

    [Function(nameof(CallSK_GetClaimHistory))]
    public static async Task<string> CallSK_GetClaimHistory([ActivityTrigger] string claimId, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(CallSK_GetClaimHistory));

        logger.LogInformation("Retrieving claim history for claim {claimId}.", claimId);

        // Simulate retrieving claim history
        var history = await Task.FromResult("Claim history data");

        return history;
    }

    [Function(nameof(CallSK_IsClaimFraudulent))]
    public static async Task<bool> CallSK_IsClaimFraudulent([ActivityTrigger] object input, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(CallSK_IsClaimFraudulent));

        logger.LogInformation("Checking if claim is fraudulent with input: {input}.", input);

        // Simulate fraud detection logic
        bool isFraudulent = false; // Replace with actual fraud detection logic

        return await Task.FromResult(isFraudulent);
    }

    [Function(nameof(CallSK_DecideAction))]
    public static async Task<string> CallSK_DecideAction([ActivityTrigger] object input, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(CallSK_DecideAction));

        logger.LogInformation("Deciding action for claim with input: {input}.", input);

        // Simulate decision-making logic
        string decision = "escalate"; // Replace with actual decision logic

        return await Task.FromResult(decision);
    }

    [Function(nameof(CallSK_GenerateClaimSummary))]
    public static async Task<string> CallSK_GenerateClaimSummary([ActivityTrigger] MyClaimData claim, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(CallSK_GenerateClaimSummary));

        logger.LogInformation("Generating claim summary for claim {claimId}.", claim.ClaimId);

        // Simulate generating a summary
        var summary = $"Summary for claim {claim.ClaimId}: {claim.AccidentDescription}";

        return await Task.FromResult(summary);
    }
}