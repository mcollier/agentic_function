using AgentFunction.ApiService.Services;
using AgentFunction.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IClaimsService, ClaimsService>();

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

// Claims history endpoint
app.MapGet("/customers/{customerId}/claims/history", async (string customerId, IClaimsService claimsService) =>
{
    if (string.IsNullOrWhiteSpace(customerId))
    {
        return Results.BadRequest("Customer ID is required");
    }

    var claims = await claimsService.GetClaimsHistoryAsync(customerId);

    // If no claims where found, return a 404 response.  Otherwise, return 200.
    if (claims == null || !claims.Any())
    {
        return Results.NotFound();
    }
    
    return Results.Ok(claims);
})
.WithName("GetCustomerClaimsHistory")
.WithSummary("Get customer claims history")
.WithDescription("Returns the historical insurance claims for a specific customer")
.Produces(200, typeof(IEnumerable<Claim>))
.Produces(400);

app.MapDefaultEndpoints();

app.MapMcp();

app.Run();
