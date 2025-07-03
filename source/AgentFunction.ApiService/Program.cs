using AgentFunction.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Register claims service
builder.Services.AddScoped<IClaimsService, ClaimsService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Agent Function API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Claims History API endpoint
app.MapGet("/customers/{customerId}/claims/history", async (string customerId, IClaimsService claimsService) =>
{
    if (string.IsNullOrWhiteSpace(customerId))
    {
        return Results.BadRequest("Customer ID is required");
    }

    var claims = await claimsService.GetCustomerClaimsHistoryAsync(customerId);
    return Results.Ok(claims);
})
.WithName("GetCustomerClaimsHistory")
.WithSummary("Get customer claims history")
.WithDescription("Returns the historical insurance claims for a specific customer")
.Produces(200, typeof(IEnumerable<AgentFunction.ApiService.Models.Claim>))
.Produces(400);

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}