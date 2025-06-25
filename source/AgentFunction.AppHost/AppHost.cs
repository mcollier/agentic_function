var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator();

var apiService = builder.AddProject<Projects.AgentFunction_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AgentFunction_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
