var builder = DistributedApplication.CreateBuilder(args);

// Get parameters to use in other resources.
// These parameters can be used to configure the application or pass values to services.
var existingAzureOpenAIResourceGroup = builder.AddParameter("existingAzureOpenAIResourceGroup");
var existingAzureOpenAIName = builder.AddParameter("existingAzureOpenAIName");
var existingAzureOpenAIModelName = builder.AddParameter("existingAzureOpenAIModelName");
var azureDurableTaskSchedulerConnectionString = builder.AddParameter("azureDurableTaskSchedulerConnectionString");
var azureDurableTaskSchedulerTaskHubName = builder.AddParameter("azureDurableTaskSchedulerTaskHubName");
var senderEmailAddress = builder.AddParameter("senderEmailAddress");
var recipientEmailAddress = builder.AddParameter("recipientEmailAddress");

var storage = builder.AddAzureStorage(Shared.Services.AzureStorage)
                     .RunAsEmulator(azurite =>
                     {
                         azurite.WithBlobPort(27000)
                                .WithQueuePort(27001)
                                .WithTablePort(27002)
                                .WithLifetime(ContainerLifetime.Persistent);
                     });
var blobs = storage.AddBlobs(Shared.Services.AzureStorageBlobs);
var queues = storage.AddQueues(Shared.Services.AzureStorageQueues);
var tables = storage.AddTables(Shared.Services.AzureStorageTables);

var azureOpenAi = builder.AddAzureOpenAI(Shared.Services.AzureOpenAI)
                         .AsExisting(existingAzureOpenAIName, existingAzureOpenAIResourceGroup);

// Existing Azure Communication Service connection string
var azureCommunicationService = builder.AddConnectionString("AzureCommunicationServiceConnectionString");

var policyApi = builder.AddProject<Projects.PolicyApi>(Shared.Services.PolicyApi)
    .WithHttpHealthCheck("/health");


var functions = builder.AddAzureFunctionsProject<Projects.FunctionsService>(Shared.Services.FunctionsService)
.WithHostStorage(storage)
.WithReference(policyApi)
.WithReference(queues)
.WithReference(blobs)
.WithReference(azureCommunicationService)
.WithReference(azureOpenAi)
.WaitFor(storage)
.WaitFor(policyApi)
.WithEnvironment("AZURE_OPENAI_DEPLOYMENT_NAME", existingAzureOpenAIModelName)
.WithEnvironment("DURABLE_TASK_SCHEDULER_CONNECTION_STRING", azureDurableTaskSchedulerConnectionString)
.WithEnvironment("TASKHUB_NAME", azureDurableTaskSchedulerTaskHubName)
.WithEnvironment("SENDER_EMAIL_ADDRESS", senderEmailAddress)
// .WithEnvironment("RECIPIENT_EMAIL_ADDRESS", recipientEmailAddress)
.WithEnvironment("MCP_SERVER_URL", policyApi.GetEndpoint("http"))
.WithExternalHttpEndpoints();

// Only use the Durable Task Scheduler in run mode.
// In local development, we use the DTS emulator.
if (builder.ExecutionContext.IsRunMode)
{
    var dts = builder.AddContainer("dts", "mcr.microsoft.com/dts/dts-emulator:latest")
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "dts-grpc")
                    .WithHttpEndpoint(port: 8082, targetPort: 8082, name: "dts-dashboard");

    functions.WaitFor(dts);
}

builder.Build().Run();
