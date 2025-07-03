using AgentFunction.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();

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

// Mock data for claims
string[] statuses = ["Pending", "UnderReview", "Approved", "Denied", "Settled"];
string[] vehicleMakes = ["Toyota", "Honda", "Ford", "Chevrolet", "BMW", "Mercedes-Benz", "Audi"];
string[] models = ["Camry", "Accord", "F-150", "Silverado", "3 Series", "C-Class", "A4"];
string[] descriptions = [
    "Rear-ended at a stoplight. Minor damage to rear bumper.",
    "Side-swiped in parking lot. Scratches along driver side door.",
    "Hit by falling tree branch during storm. Dent in roof.",
    "Collision at intersection. Front-end damage.",
    "Vandalism - keyed along passenger side.",
    "Hit-and-run incident. Damage to front quarter panel.",
    "Minor fender bender in parking garage."
];
string[] firstNames = ["John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Jennifer"];
string[] lastNames = ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis"];
string[] domains = ["email.com", "gmail.com", "outlook.com", "yahoo.com"];

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
app.MapGet("/customers/{customerId}/claims/history", (string customerId) =>
{
    if (string.IsNullOrWhiteSpace(customerId))
    {
        return Results.BadRequest("Customer ID is required");
    }

    // Generate mock claims based on customer ID for consistency
    var random = new Random(customerId.GetHashCode());
    var claimsCount = random.Next(1, 5); // 1-4 claims per customer

    var claims = new List<Claim>();
    
    for (int i = 0; i < claimsCount; i++)
    {
        var claimDate = DateTime.UtcNow.AddDays(-random.Next(30, 365)); // Claims from last 30-365 days
        
        // Generate random name
        var firstName = firstNames[random.Next(firstNames.Length)];
        var lastName = lastNames[random.Next(lastNames.Length)];
        var fullName = $"{firstName} {lastName}";
        var email = $"{firstName.ToLower()}.{lastName.ToLower()}@{domains[random.Next(domains.Length)]}";
        
        // Generate random license plate
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        var licensePlate = new string(Enumerable.Repeat(letters, 3)
            .Select(s => s[random.Next(s.Length)])
            .Concat(Enumerable.Repeat(numbers, 4)
                .Select(s => s[random.Next(s.Length)]))
            .ToArray());
        
        var claim = new Claim(
            ClaimId: $"CLM-{claimDate:yyyyMMdd}-{(i + 1):D3}",
            PolicyNumber: $"POL-{random.Next(100000000, 999999999)}",
            ClaimantName: fullName,
            ClaimantContact: email,
            DateOfAccident: claimDate,
            AccidentDescription: descriptions[random.Next(descriptions.Length)],
            VehicleMake: vehicleMakes[random.Next(vehicleMakes.Length)],
            VehicleModel: models[random.Next(models.Length)],
            VehicleYear: random.Next(2015, 2025),
            LicensePlate: licensePlate,
            AmountClaimed: Math.Round((decimal)(random.NextDouble() * 15000 + 500), 2), // $500-$15,500
            Status: statuses[random.Next(statuses.Length)]
        );
        
        claims.Add(claim);
    }

    // Sort by claim date descending (most recent first)
    var sortedClaims = claims.OrderByDescending(c => c.DateOfAccident);
    return Results.Ok(sortedClaims);
})
.WithName("GetCustomerClaimsHistory")
.WithSummary("Get customer claims history")
.WithDescription("Returns the historical insurance claims for a specific customer")
.Produces(200, typeof(IEnumerable<Claim>))
.Produces(400);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}