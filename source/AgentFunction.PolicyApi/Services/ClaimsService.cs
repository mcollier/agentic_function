using AgentFunction.Models;

namespace AgentFunction.ApiService.Services;

public class ClaimsService : IClaimsService
{
    private static readonly List<ClaimHistory> CustomerClaims =
    [
        new ClaimHistory
        {
            Customer = new Customer
            {
                CustomerId = "32-445-8382",
                Name = "John Smith",
                ContactInfo = null
            },
            Claims =
            [
                new ClaimDetail
                {
                    ClaimId = "CLM-20240101-001",
                    PolicyNumber = "POL-123456789",
                    DateOfAccident = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                    AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                    VehicleMake = "Toyota",
                    VehicleModel = "Camry",
                    VehicleYear = 2022,
                    LicensePlate = "ABC1234",
                    AmountClaimed = 2500.00m,
                    Status = ClaimStatus.Approved
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240115-002",
                    PolicyNumber = "POL-123456789",
                    DateOfAccident = new DateTime(2024, 1, 20, 10, 0, 0, DateTimeKind.Utc),
                    AccidentDescription = "Hit a pothole causing tire damage.",
                    VehicleMake = "Toyota",
                    VehicleModel = "Camry",
                    VehicleYear = 2022,
                    LicensePlate = "ABC1234",
                    AmountClaimed = 300.00m,
                    Status = ClaimStatus.Submitted
                }
            ]
        },
        new ClaimHistory
        {
            Customer = new Customer
            {
                CustomerId = "45-123-7890",
                Name = "Alice Johnson",
                ContactInfo = null
            },
            Claims =
            [
                new ClaimDetail
                {
                    ClaimId = "CLM-20240210-002",
                    PolicyNumber = "POL-987654321",
                    DateOfAccident = new DateTime(2024, 2, 10, 9, 0, 0, DateTimeKind.Utc),
                    AccidentDescription = "Side collision at intersection. Door replacement required.",
                    VehicleMake = "Honda",
                    VehicleModel = "Civic",
                    VehicleYear = 2021,
                    LicensePlate = "XYZ5678",
                    AmountClaimed = 3200.00m,
                    Status = ClaimStatus.Submitted
                }
            ]
        },
        new ClaimHistory
        {
            Customer = new Customer
            {
                CustomerId = "56-789-1234",
                Name = "Robert Lee",
                ContactInfo = null
            },
            Claims=
            [
                new ClaimDetail
                {
                    ClaimId = "CLM-20240122-006",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 1, 22, 16, 45, 0, DateTimeKind.Utc),
                    AccidentDescription = "Windshield cracked by flying debris.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 800.00m,
                    Status = ClaimStatus.Approved
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240125-007",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 1, 25, 13, 10, 0, DateTimeKind.Utc),
                    AccidentDescription = "Rear bumper scratched in parking lot.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 400.00m,
                    Status = ClaimStatus.Submitted
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240201-008",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 2, 1, 7, 30, 0, DateTimeKind.Utc),
                    AccidentDescription = "Flat tire due to nail on driveway.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 120.00m,
                    Status = ClaimStatus.Submitted
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240210-009",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 2, 10, 18, 0, 0, DateTimeKind.Utc),
                    AccidentDescription = "Side mirror broken by passing vehicle.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 250.00m,
                    Status = ClaimStatus.Submitted
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240215-010",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 2, 15, 20, 20, 0, DateTimeKind.Utc),
                    AccidentDescription = "Minor fire damage to engine compartment.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 2000.00m,
                    Status = ClaimStatus.Rejected
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240220-011",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 2, 20, 12, 0, 0, DateTimeKind.Utc),
                    AccidentDescription = "Broken tail light after reversing into pole.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 180.00m,
                    Status = ClaimStatus.Approved
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240225-012",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 2, 25, 15, 50, 0, DateTimeKind.Utc),
                    AccidentDescription = "Paint damage from vandalism.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 950.00m,
                    Status = ClaimStatus.Submitted
                },
                new ClaimDetail
                {
                    ClaimId = "CLM-20240120-004",
                    PolicyNumber = "POL-555555555",
                    DateOfAccident = new DateTime(2024, 1, 20, 11, 15, 0, DateTimeKind.Utc),
                    AccidentDescription = "Hail damage to hood and roof.",
                    VehicleMake = "Ford",
                    VehicleModel = "F-150",
                    VehicleYear = 2020,
                    LicensePlate = "TRK2020",
                    AmountClaimed = 1500.00m,
                    Status = ClaimStatus.Rejected
                }
            ]
        },
        new ClaimHistory
        {
            Customer = new Customer
            {
                CustomerId = "78-654-3210",
                Name = "Maria Garcia",
                ContactInfo = null
            },
            Claims=
            [
                new ClaimDetail
                {
                    ClaimId = "CLM-20240215-005",
                    PolicyNumber = "POL-333444555",
                    DateOfAccident = new DateTime(2024, 2, 15, 8, 30, 0, DateTimeKind.Utc),
                    AccidentDescription = "Collision with deer on rural road.",
                    VehicleMake = "Chevrolet",
                    VehicleModel = "Malibu",
                    VehicleYear = 2019,
                    LicensePlate = "CHEVY19",
                    AmountClaimed = 600.00m,
                    Status = ClaimStatus.Submitted
                }
            ]
        }
    ];

    public Task<IEnumerable<ClaimDetail>> GetClaimsHistoryAsync(string customerId)
    {
        var claimsHistory = CustomerClaims.Where(c => c.Customer.CustomerId == customerId);
        if (claimsHistory.Any())
        {
            var claimsDetails = claimsHistory.SelectMany(c => c.Claims);
            // var sortedClaims = claimsHistory.OrderByDescending(c => c.Claims.First().DateOfAccident);
            return Task.FromResult(claimsDetails);
        }

        return Task.FromResult<IEnumerable<ClaimDetail>>([]);
    }
}
