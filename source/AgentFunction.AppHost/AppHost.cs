var builder = DistributedApplication.CreateBuilder(args);

// Azure OpenAI
var existingAzureAiFoundryName = builder.AddParameter("existingAzureAiFoundryName");
var existingAzureOpenAIResourceGroup = builder.AddParameter("existingAzureOpenAIResourceGroup");

var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator();
var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

// Add Azure AI Foundry project
var foundry = builder.AddAzureAIFoundry("foundry")
                     .AsExisting(existingAzureAiFoundryName, existingAzureOpenAIResourceGroup);

var chat = foundry.AddDeployment("chat", "gpt-4o-mini", "2024-07-18", "OpenAI");

var apiService = builder.AddProject<Projects.AgentFunction_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

var functions = builder.AddAzureFunctionsProject<Projects.AgentFunction_Functions>("functions")
    .WaitFor(storage)
    .WithHostStorage(storage)
    .WithReference(chat)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.AgentFunction_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(functions)
    .WaitFor(apiService)
    .WaitFor(functions);

builder.Build().Run();
