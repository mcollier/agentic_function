using AgentFunction.Functions.Models;

using Azure.Storage.Blobs;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions.Activities;

public class FinalizeActivity(BlobServiceClient blobServiceClient)
{
    [Function(nameof(FinalizeClaim))]
    public async Task FinalizeClaim(
        [ActivityTrigger] ClaimAnalysisReport input,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(FinalizeClaim));
        logger.LogInformation("Finalizing claim...");

        var basePath = $"claims/{input.ClaimId}";

        var containerClient = blobServiceClient.GetBlobContainerClient("claims");
        await containerClient.CreateIfNotExistsAsync();

        var reportBlobClient = containerClient.GetBlobClient($"{basePath}/report.json");
        await reportBlobClient.UploadAsync(BinaryData.FromObjectAsJson(input), overwrite: true);

        var communicationsBlobClient = containerClient.GetBlobClient($"{basePath}/customer_communication.json");
        await communicationsBlobClient.UploadAsync(BinaryData.FromObjectAsJson(input.Communications), overwrite: true);

        // await client.UploadBlobAsync($"{basePath}/report.json", BinaryData.FromObjectAsJson(input));
        // await client.UploadBlobAsync($"{basePath}/customer_communication.json", BinaryData.FromObjectAsJson(input.Communications));
    }
}