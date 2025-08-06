using System.ComponentModel;

var builder = DistributedApplication.CreateBuilder(args);

// Azure OpenAI
var existingAzureAiFoundryName = builder.AddParameter("existingAzureAiFoundryName");
var existingAzureAiFoundryResourceGroup = builder.AddParameter("existingAzureAiFoundryResourceGroup");
var existingAzureOpenAIResourceGroup = builder.AddParameter("existingAzureOpenAIResourceGroup");
var existingAzureOpenAIName = builder.AddParameter("existingAzureOpenAIName");
var azureCommunicationServiceConnectionString = builder.AddParameter("azureCommunicationServiceConnectionString");
var azureDurableTaskSchedulerConnectionString = builder.AddParameter("azureDurableTaskSchedulerConnectionString");
var azureDurableTaskSchedulerTaskHubName = builder.AddParameter("azureDurableTaskSchedulerTaskHubName");
var senderEmailAddress = builder.AddParameter("senderEmailAddress");

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

// TODO: Update to use Azure Durable Task Scheduler resource in Azure
//       Using DTS emulator only for local development.
var dts = builder.AddContainer("dts", "mcr.microsoft.com/dts/dts-emulator:latest")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "dts-grpc")
                 .WithHttpEndpoint(port: 8082, targetPort: 8082, name: "dts-dashboard");


var azureOpenAi = builder.AddAzureOpenAI(Shared.Services.AzureOpenAI)
                         .AsExisting(existingAzureOpenAIName, existingAzureOpenAIResourceGroup);

var apiService = builder.AddProject<Projects.ApiService>(Shared.Services.ApiService)
    .WithHttpHealthCheck("/health");

var claimsAgent = builder.AddProject<Projects.ClaimsAgentService>(Shared.Services.ClaimsAgentService)
    .WithHttpHealthCheck("/health")
    .WithReference(azureOpenAi);

var functions = builder.AddAzureFunctionsProject<Projects.FunctionsService>(Shared.Services.FunctionsService)
.WithHostStorage(storage)
.WithReference(claimsAgent)
.WithReference(apiService)
.WithReference(queues)
.WaitFor(storage)
.WaitFor(claimsAgent)
.WaitFor(apiService)
.WaitFor(dts)
.WithEnvironment("DURABLE_TASK_SCHEDULER_CONNECTION_STRING", azureDurableTaskSchedulerConnectionString)
.WithEnvironment("TASKHUB_NAME", azureDurableTaskSchedulerTaskHubName)
.WithEnvironment("COMMUNICATION_SERVICES_CONNECTION_STRING", azureCommunicationServiceConnectionString)
.WithEnvironment("SENDER_EMAIL_ADDRESS", senderEmailAddress)
.WithExternalHttpEndpoints();

builder.Build().Run();
