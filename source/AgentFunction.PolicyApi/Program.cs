using AgentFunction.ApiService.Models;
using AgentFunction.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IClaimsService, ClaimsService>();
builder.Services.AddScoped<IClaimHistoryService, ClaimHistoryService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add MCP support
builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Policy claims endpoint
app.MapGet("/api/policy/{policyId}/claims", async (string policyId, IClaimHistoryService claimHistoryService) =>
{
    if (string.IsNullOrWhiteSpace(policyId))
    {
        return Results.BadRequest("Policy ID is required");
    }

    var claims = await claimHistoryService.GetClaimsByPolicyIdAsync(policyId);

    // Return 200 with claims (could be empty collection)
    return Results.Ok(claims);
})
.WithName("GetPolicyClaimsHistory")
.WithSummary("Get policy claims history")
.WithDescription("Returns all claims associated with a specific policy ID")
.Produces<IEnumerable<ClaimHistory>>(200)
.Produces(400);


app.MapDefaultEndpoints();

app.MapMcp();

app.Run();
