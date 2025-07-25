using System.Net;
using AgentFunction.Functions;
using AgentFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

public class ClaimProcessApi
{
    [Function(nameof(StartClaimProcess))]
    public async Task<HttpResponseData> StartClaimProcess(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(StartClaimProcess));
        logger.LogInformation("Received request to start claim process.");

        Claim claimRequest;
        try
        {
            // Validate and deserialize the request
            claimRequest = await req.ReadFromJsonAsync<Claim>()
                ?? throw new InvalidOperationException("Request body is null or invalid.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read or deserialize request body.");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteStringAsync("Invalid request body.");
            return errorResponse;
        }

        // Start the orchestration
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AgentOrchestrator.ClaimProcessingOrchestration), claimRequest);
        logger.LogInformation("Started orchestration with ID: {instanceId}", instanceId);

        // Return the response
        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.Headers.Add("Location", $"/api/claims/status/{instanceId}");

        await response.WriteAsJsonAsync(new
        {
            id = instanceId,
            statusQueryUri = $"/api/claims/status/{instanceId}",
        });

        return response;
    }

    [Function(nameof(GetClaimStatus))]
    public async Task<HttpResponseData> GetClaimStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "claims/status/{instanceId}")] HttpRequestData req,
        string instanceId,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GetClaimStatus));
        logger.LogInformation("Received request to get status for instance ID: {instanceId}", instanceId);

        // Get the status of the orchestration instance
        var status = await client.GetInstanceAsync(instanceId);
        logger.LogInformation("Orchestration status for ID {instanceId}: {status}", instanceId, status?.RuntimeStatus.ToString() ?? "NotFound");


        if (status == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"No orchestration found with instance Id = {instanceId}");
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(status);
        return response;
    }
}