using AgentFunction.ApiService.Models;

namespace AgentFunction.ApiService.Services;

/// <summary>
/// Service for retrieving claim history by policy ID.
/// </summary>
public class ClaimHistoryService : IClaimHistoryService
{
    // Hardcoded sample data across multiple policy IDs for testing/demo purposes
    private static readonly List<ClaimHistory> SampleClaims =
    [
        new ClaimHistory
        {
            ClaimId = "C-1001",
            PolicyId = "P-98765",
            LossDate = new DateTimeOffset(new DateTime(2025, 3, 12, 14, 45, 0, DateTimeKind.Utc)),
            ReportDate = new DateTimeOffset(new DateTime(2025, 3, 13, 9, 10, 0, DateTimeKind.Utc)),
            Vehicle = new ClaimHistoryVehicle
            {
                Year = 2019,
                Make = "Honda",
                Model = "Civic",
                Trim = "LX"
            },
            Location = new ClaimHistoryLocation
            {
                Line1 = "456 Oak Ave.",
                City = "Columbus",
                State = "OH",
                PostalCode = "43215"
            },
            Parties =
            [
                new ClaimHistoryParty
                {
                    Role = "Insured",
                    Name = "Alice Johnson",
                    Contact = new ClaimHistoryContact
                    {
                        Phone = "555-555-1234",
                        Email = "alice.johnson@example.com"
                    }
                },
                new ClaimHistoryParty
                {
                    Role = "ThirdParty",
                    Name = "Bob Smith",
                    Contact = new ClaimHistoryContact
                    {
                        Phone = "555-555-5678",
                        Email = "bob.smith@example.com"
                    }
                }
            ],
            Vendor = "QuickFix Auto Repair",
            Payout = 1800.00M,
            Status = ClaimStatus.Closed
        },
        new ClaimHistory
        {
            ClaimId = "C-1007",
            PolicyId = "P-98765",
            LossDate = new DateTimeOffset(new DateTime(2025, 06, 28, 16, 15, 0, DateTimeKind.Utc)),
            ReportDate = new DateTimeOffset(new DateTime(2025, 07, 5, 08, 20, 0, DateTimeKind.Utc)),
            Vehicle = new ClaimHistoryVehicle
            {
                Year = 2019,
                Make = "Honda",
                Model = "Civic",
                Trim = "LX"
            },
            Location = new ClaimHistoryLocation
            {
                Line1 = "789 Pine St.",
                City = "Dayton",
                State = "OH",
                PostalCode = "45402"
            },
            Parties =
            [
                new ClaimHistoryParty
                {
                    Role = "Insured",
                    Name = "Alice Johnson",
                    Contact = new ClaimHistoryContact
                    {
                        Phone = "555-555-1234",
                        Email = "alice.johnson@example.com"
                    }
                }
            ],
            Vendor = "QuickFix Auto Repair",
            Payout = 2200.00M,
            Status = ClaimStatus.Closed
        },
        new ClaimHistory
        {
            ClaimId = "C-1010",
            PolicyId = "P-98765",
            LossDate = new DateTimeOffset(new DateTime(2025, 07, 12, 19, 30, 0, DateTimeKind.Utc)),
            ReportDate = new DateTimeOffset(new DateTime(2025, 07, 30, 11, 00, 0, DateTimeKind.Utc)),
            Vehicle = new ClaimHistoryVehicle
            {
                Year = 2019,
                Make = "Honda",
                Model = "Civic",
                Trim = "LX"
            },
            Location = new ClaimHistoryLocation
            {
                Line1 = "135 Maple Rd.",
                City = "Cincinnati",
                State = "OH",
                PostalCode = "45202"
            },
            Parties =
            [
                new ClaimHistoryParty
                {
                    Role = "Insured",
                    Name = "Alice Johnson",
                    Contact = new ClaimHistoryContact
                    {
                        Phone = "555-555-9876",
                        Email = "alice.johnson@example.com"
                    }
                }
            ],
            Vendor = "QuickFix Auto Repair",
            Payout = 1950.00M,
            Status = ClaimStatus.Open
        },
        new ClaimHistory
        {
            ClaimId = "C-1002",
            PolicyId = "P-12345",
            LossDate = new DateTimeOffset(new DateTime(2024, 11, 5, 10, 0, 0, DateTimeKind.Utc)),
            ReportDate = new DateTimeOffset(new DateTime(2024, 11, 6, 14, 30, 0, DateTimeKind.Utc)),
            Vehicle = new ClaimHistoryVehicle
            {
                Year = 2020,
                Make = "Toyota",
                Model = "Camry",
                Trim = "SE"
            },
            Location = new ClaimHistoryLocation
            {
                Line1 = "123 Main St.",
                City = "Springfield",
                State = "IL",
                PostalCode = "62701"
            },
            Parties =
            [
                new ClaimHistoryParty
                {
                    Role = "Insured",
                    Name = "John Doe",
                    Contact = new ClaimHistoryContact
                    {
                        Phone = "555-555-1234",
                        Email = ""
                    }
                }
            ],
            Vendor = "AutoFix Repair Shop",
            Payout = 2500.00M,
            Status = ClaimStatus.Closed
        }
    ];

    /// <summary>
    /// Retrieves all claims associated with a specific policy ID.
    /// Performs case-insensitive matching on the policy ID.
    /// </summary>
    /// <param name="policyId">The policy ID to search for</param>
    /// <returns>A collection of claims for the specified policy. Returns empty collection if no matches found.</returns>
    public Task<IEnumerable<ClaimHistory>> GetClaimsByPolicyIdAsync(string policyId)
    {
        if (string.IsNullOrWhiteSpace(policyId))
        {
            return Task.FromResult<IEnumerable<ClaimHistory>>([]);
        }

        // Perform case-insensitive matching on policy ID
        var matchingClaims = SampleClaims
            .Where(claim => claim.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(claim => claim.LossDate);

        return Task.FromResult<IEnumerable<ClaimHistory>>(matchingClaims);
    }
}