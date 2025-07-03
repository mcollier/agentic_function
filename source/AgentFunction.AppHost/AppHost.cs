var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator();
var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

var apiService = builder.AddProject<Projects.AgentFunction_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

var functions = builder.AddAzureFunctionsProject<Projects.AgentFunction_Functions>("functions")
    .WaitFor(storage)
    .WithHostStorage(storage)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.AgentFunction_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(functions)
    .WaitFor(apiService)
    .WaitFor(functions);

builder.Build().Run();
