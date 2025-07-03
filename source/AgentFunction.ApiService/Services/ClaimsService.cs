using AgentFunction.ApiService.Models;

namespace AgentFunction.ApiService.Services;

public class ClaimsService : IClaimsService
{
    // Mock data for claims
    private static readonly string[] Statuses = ["Pending", "UnderReview", "Approved", "Denied", "Settled"];
    private static readonly string[] VehicleMakes = ["Toyota", "Honda", "Ford", "Chevrolet", "BMW", "Mercedes-Benz", "Audi"];
    private static readonly string[] Models = ["Camry", "Accord", "F-150", "Silverado", "3 Series", "C-Class", "A4"];
    private static readonly string[] Descriptions = [
        "Rear-ended at a stoplight. Minor damage to rear bumper.",
        "Side-swiped in parking lot. Scratches along driver side door.",
        "Hit by falling tree branch during storm. Dent in roof.",
        "Collision at intersection. Front-end damage.",
        "Vandalism - keyed along passenger side.",
        "Hit-and-run incident. Damage to front quarter panel.",
        "Minor fender bender in parking garage."
    ];
    private static readonly string[] FirstNames = ["John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Jennifer"];
    private static readonly string[] LastNames = ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis"];
    private static readonly string[] Domains = ["email.com", "gmail.com", "outlook.com", "yahoo.com"];

    public Task<IEnumerable<Claim>> GetClaimsHistoryAsync(string customerId)
    {
        // Generate mock claims based on customer ID for consistency
        var random = new Random(customerId.GetHashCode());
        var claimsCount = random.Next(1, 5); // 1-4 claims per customer

        var claims = new List<Claim>();
        
        for (int i = 0; i < claimsCount; i++)
        {
            var claimDate = DateTime.UtcNow.AddDays(-random.Next(30, 365)); // Claims from last 30-365 days
            
            // Generate random name
            var firstName = FirstNames[random.Next(FirstNames.Length)];
            var lastName = LastNames[random.Next(LastNames.Length)];
            var fullName = $"{firstName} {lastName}";
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@{Domains[random.Next(Domains.Length)]}";
            
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
                AccidentDescription: Descriptions[random.Next(Descriptions.Length)],
                VehicleMake: VehicleMakes[random.Next(VehicleMakes.Length)],
                VehicleModel: Models[random.Next(Models.Length)],
                VehicleYear: random.Next(2015, 2025),
                LicensePlate: licensePlate,
                AmountClaimed: Math.Round((decimal)(random.NextDouble() * 15000 + 500), 2), // $500-$15,500
                Status: Statuses[random.Next(Statuses.Length)]
            );
            
            claims.Add(claim);
        }

        // Sort by claim date descending (most recent first)
        var sortedClaims = claims.OrderByDescending(c => c.DateOfAccident);
        return Task.FromResult<IEnumerable<Claim>>(sortedClaims);
    }
}