using AgentFunction.Models;
using AgentFunction.Models.Dtos;

namespace AgentFunction.ApiService.Services;

/// <summary>
/// Service for retrieving claim history by policy ID.
/// </summary>
public class ClaimHistoryService : IClaimHistoryService
{
    // Hardcoded sample data across multiple policy IDs for testing/demo purposes
    private static readonly List<ClaimDto> SampleClaims =
    [
        // Policy POL-123456789 claims
        new ClaimDto
        {
            ClaimId = "CLM-20240101-001",
            PolicyNumber = "POL-123456789",
            DateOfAccident = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc),
            AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
            Vehicle = "Toyota Camry",
            VehicleYear = 2022,
            AmountClaimed = 2500.00m,
            Status = ClaimStatus.Approved
        },
        new ClaimDto
        {
            ClaimId = "CLM-20240115-002",
            PolicyNumber = "POL-123456789",
            DateOfAccident = new DateTime(2024, 1, 20, 10, 0, 0, DateTimeKind.Utc),
            AccidentDescription = "Hit a pothole causing tire damage.",
            Vehicle = "Toyota Camry",
            VehicleYear = 2022,
            AmountClaimed = 300.00m,
            Status = ClaimStatus.Submitted
        },

        // Policy POL-987654321 claims
        new ClaimDto
        {
            ClaimId = "CLM-20240210-002",
            PolicyNumber = "POL-987654321",
            DateOfAccident = new DateTime(2024, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            AccidentDescription = "Side collision at intersection. Door replacement required.",
            Vehicle = "Honda Civic",
            VehicleYear = 2021,
            AmountClaimed = 3200.00m,
            Status = ClaimStatus.Submitted
        },

        // Policy POL-555555555 claims (multiple claims for testing)
        new ClaimDto
        {
            ClaimId = "CLM-20240122-006",
            PolicyNumber = "POL-555555555",
            DateOfAccident = new DateTime(2024, 1, 22, 16, 45, 0, DateTimeKind.Utc),
            AccidentDescription = "Windshield cracked by flying debris.",
            Vehicle = "Ford F-150",
            VehicleYear = 2020,
            AmountClaimed = 800.00m,
            Status = ClaimStatus.Approved
        },
        new ClaimDto
        {
            ClaimId = "CLM-20240125-007",
            PolicyNumber = "POL-555555555",
            DateOfAccident = new DateTime(2024, 1, 25, 13, 10, 0, DateTimeKind.Utc),
            AccidentDescription = "Rear bumper scratched in parking lot.",
            Vehicle = "Ford F-150",
            VehicleYear = 2020,
            AmountClaimed = 400.00m,
            Status = ClaimStatus.Submitted
        },
        new ClaimDto
        {
            ClaimId = "CLM-20240201-008",
            PolicyNumber = "POL-555555555",
            DateOfAccident = new DateTime(2024, 2, 1, 7, 30, 0, DateTimeKind.Utc),
            AccidentDescription = "Flat tire due to nail on driveway.",
            Vehicle = "Ford F-150",
            VehicleYear = 2020,
            AmountClaimed = 120.00m,
            Status = ClaimStatus.Submitted
        },

        // Policy POL-333444555 claims
        new ClaimDto
        {
            ClaimId = "CLM-20240215-005",
            PolicyNumber = "POL-333444555",
            DateOfAccident = new DateTime(2024, 2, 15, 8, 30, 0, DateTimeKind.Utc),
            AccidentDescription = "Collision with deer on rural road.",
            Vehicle = "Chevrolet Malibu",
            VehicleYear = 2019,
            AmountClaimed = 600.00m,
            Status = ClaimStatus.Submitted
        }
    ];

    /// <summary>
    /// Retrieves all claims associated with a specific policy ID.
    /// Performs case-insensitive matching on the policy ID.
    /// </summary>
    /// <param name="policyId">The policy ID to search for</param>
    /// <returns>A collection of claims for the specified policy. Returns empty collection if no matches found.</returns>
    public Task<IEnumerable<ClaimDto>> GetClaimsByPolicyIdAsync(string policyId)
    {
        if (string.IsNullOrWhiteSpace(policyId))
        {
            return Task.FromResult<IEnumerable<ClaimDto>>([]);
        }

        // Perform case-insensitive matching on policy ID
        var matchingClaims = SampleClaims
            .Where(claim => claim.PolicyNumber.Equals(policyId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(claim => claim.DateOfAccident);

        return Task.FromResult<IEnumerable<ClaimDto>>(matchingClaims);
    }
}