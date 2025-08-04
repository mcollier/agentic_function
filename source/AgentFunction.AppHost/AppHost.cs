var builder = DistributedApplication.CreateBuilder(args);

// Azure OpenAI
var existingAzureAiFoundryName = builder.AddParameter("existingAzureAiFoundryName");
var existingAzureAiFoundryResourceGroup = builder.AddParameter("existingAzureAiFoundryResourceGroup");
var existingAzureOpenAIResourceGroup = builder.AddParameter("existingAzureOpenAIResourceGroup");
var existingAzureOpenAIName = builder.AddParameter("existingAzureOpenAIName");
var azureCommunicationServiceConnectionString = builder.AddParameter("azureCommunicationServiceConnectionString");
var senderEmailAddress = builder.AddParameter("senderEmailAddress");

var storage = builder.AddAzureStorage(Shared.Services.AzureStorage)
                     .RunAsEmulator();
var blobs = storage.AddBlobs(Shared.Services.AzureStorageBlobs);
var queues = storage.AddQueues(Shared.Services.AzureStorageQueues);

// Add Azure AI Foundry project
// var foundry = builder.AddAzureAIFoundry("foundry")
//                      .AsExisting(existingAzureAiFoundryName, existingAzureAiFoundryResourceGroup);
// var chat = foundry.AddDeployment("chat", "gpt-4o-mini", "2024-07-18", "OpenAI");

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
.WithEnvironment("COMMUNICATION_SERVICES_CONNECTION_STRING", azureCommunicationServiceConnectionString)
.WithEnvironment("SENDER_EMAIL_ADDRESS", senderEmailAddress)
.WithExternalHttpEndpoints();

builder.Build().Run();
