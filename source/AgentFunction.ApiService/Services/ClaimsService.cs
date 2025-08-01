
using AgentFunction.Models;

namespace AgentFunction.ApiService.Services;

public class ClaimsService : IClaimsService
{
    // Hard-coded customer IDs and their associated claims
    private static readonly Dictionary<string, List<Claim>> CustomerClaims = new()
    {
        ["32-445-8382"] = [
            new Claim
            {
                ClaimId = "CLM-20240101-001",
                PolicyNumber = "POL-123456789",
                ClaimantName = "John Smith",
                ClaimantContact = "john.smith@email.com",
                DateOfAccident = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2022,
                LicensePlate = "ABC1234",
                AmountClaimed = 2500.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240201-002",
                PolicyNumber = "POL-123456789",
                ClaimantName = "John Smith",
                ClaimantContact = "john.smith@email.com",
                DateOfAccident = new DateTime(2024, 2, 20, 10, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2022,
                LicensePlate = "ABC1234",
                AmountClaimed = 4200.50m,
                Status = ClaimStatus.Closed
            }
        ],
        ["45-678-9012"] = [
            new Claim
            {
                ClaimId = "CLM-20230915-003",
                PolicyNumber = "POL-987654321",
                ClaimantName = "Jane Johnson",
                ClaimantContact = "jane.johnson@gmail.com",
                DateOfAccident = new DateTime(2023, 9, 15, 16, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2021,
                LicensePlate = "DEF5678",
                AmountClaimed = 1800.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20231010-004",
                PolicyNumber = "POL-987654321",
                ClaimantName = "Jane Johnson",
                ClaimantContact = "jane.johnson@gmail.com",
                DateOfAccident = new DateTime(2023, 10, 10, 8, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2021,
                LicensePlate = "DEF5678",
                AmountClaimed = 8750.25m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20231205-005",
                PolicyNumber = "POL-987654321",
                ClaimantName = "Jane Johnson",
                ClaimantContact = "jane.johnson@gmail.com",
                DateOfAccident = new DateTime(2023, 12, 5, 12, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2021,
                LicensePlate = "DEF5678",
                AmountClaimed = 950.00m,
                Status = ClaimStatus.Rejected
            }
        ],
        ["67-890-1234"] = [
            new Claim
            {
                ClaimId = "CLM-20240301-006",
                PolicyNumber = "POL-456789012",
                ClaimantName = "Michael Williams",
                ClaimantContact = "m.williams@outlook.com",
                DateOfAccident = new DateTime(2024, 3, 1, 7, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2023,
                LicensePlate = "GHI9012",
                AmountClaimed = 5600.75m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240315-007",
                PolicyNumber = "POL-456789012",
                ClaimantName = "Michael Williams",
                ClaimantContact = "m.williams@outlook.com",
                DateOfAccident = new DateTime(2024, 3, 15, 13, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2023,
                LicensePlate = "GHI9012",
                AmountClaimed = 1200.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240420-008",
                PolicyNumber = "POL-456789012",
                ClaimantName = "Michael Williams",
                ClaimantContact = "m.williams@outlook.com",
                DateOfAccident = new DateTime(2024, 4, 20, 17, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2023,
                LicensePlate = "GHI9012",
                AmountClaimed = 2100.50m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240505-009",
                PolicyNumber = "POL-456789012",
                ClaimantName = "Michael Williams",
                ClaimantContact = "m.williams@outlook.com",
                DateOfAccident = new DateTime(2024, 5, 5, 11, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2023,
                LicensePlate = "GHI9012",
                AmountClaimed = 1650.25m,
                Status = ClaimStatus.UnderReview
            }
        ],
        ["89-012-3456"] = [
            new Claim
            {
                ClaimId = "CLM-20231120-010",
                PolicyNumber = "POL-789012345",
                ClaimantName = "Sarah Brown",
                ClaimantContact = "sarah.brown@yahoo.com",
                DateOfAccident = new DateTime(2023, 11, 20, 9, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Chevrolet",
                VehicleModel = "Silverado",
                VehicleYear = 2020,
                LicensePlate = "JKL3456",
                AmountClaimed = 12500.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240110-011",
                PolicyNumber = "POL-789012345",
                ClaimantName = "Sarah Brown",
                ClaimantContact = "sarah.brown@yahoo.com",
                DateOfAccident = new DateTime(2024, 1, 10, 15, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Chevrolet",
                VehicleModel = "Silverado",
                VehicleYear = 2020,
                LicensePlate = "JKL3456",
                AmountClaimed = 875.50m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20240225-012",
                PolicyNumber = "POL-789012345",
                ClaimantName = "Sarah Brown",
                ClaimantContact = "sarah.brown@yahoo.com",
                DateOfAccident = new DateTime(2024, 2, 25, 14, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Chevrolet",
                VehicleModel = "Silverado",
                VehicleYear = 2020,
                LicensePlate = "JKL3456",
                AmountClaimed = 3200.00m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240330-013",
                PolicyNumber = "POL-789012345",
                ClaimantName = "Sarah Brown",
                ClaimantContact = "sarah.brown@yahoo.com",
                DateOfAccident = new DateTime(2024, 3, 30, 6, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Chevrolet",
                VehicleModel = "Silverado",
                VehicleYear = 2020,
                LicensePlate = "JKL3456",
                AmountClaimed = 980.75m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240415-014",
                PolicyNumber = "POL-789012345",
                ClaimantName = "Sarah Brown",
                ClaimantContact = "sarah.brown@yahoo.com",
                DateOfAccident = new DateTime(2024, 4, 15, 18, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Chevrolet",
                VehicleModel = "Silverado",
                VehicleYear = 2020,
                LicensePlate = "JKL3456",
                AmountClaimed = 6800.25m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20240510-015",
                PolicyNumber = "POL-789012345",
                ClaimantName = "Sarah Brown",
                ClaimantContact = "sarah.brown@yahoo.com",
                DateOfAccident = new DateTime(2024, 5, 10, 12, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Chevrolet",
                VehicleModel = "Silverado",
                VehicleYear = 2020,
                LicensePlate = "JKL3456",
                AmountClaimed = 1850.00m,
                Status = ClaimStatus.Approved
            }
        ],
        ["12-345-6789"] = [
            new Claim
            {
                ClaimId = "CLM-20240101-016",
                PolicyNumber = "POL-345678901",
                ClaimantName = "David Jones",
                ClaimantContact = "david.jones@email.com",
                DateOfAccident = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "BMW",
                VehicleModel = "3 Series",
                VehicleYear = 2024,
                LicensePlate = "MNO6789",
                AmountClaimed = 3400.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240215-017",
                PolicyNumber = "POL-345678901",
                ClaimantName = "David Jones",
                ClaimantContact = "david.jones@email.com",
                DateOfAccident = new DateTime(2024, 2, 15, 5, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "BMW",
                VehicleModel = "3 Series",
                VehicleYear = 2024,
                LicensePlate = "MNO6789",
                AmountClaimed = 15200.75m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240320-018",
                PolicyNumber = "POL-345678901",
                ClaimantName = "David Jones",
                ClaimantContact = "david.jones@email.com",
                DateOfAccident = new DateTime(2024, 3, 20, 16, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "BMW",
                VehicleModel = "3 Series",
                VehicleYear = 2024,
                LicensePlate = "MNO6789",
                AmountClaimed = 2100.50m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20240425-019",
                PolicyNumber = "POL-345678901",
                ClaimantName = "David Jones",
                ClaimantContact = "david.jones@email.com",
                DateOfAccident = new DateTime(2024, 4, 25, 11, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "BMW",
                VehicleModel = "3 Series",
                VehicleYear = 2024,
                LicensePlate = "MNO6789",
                AmountClaimed = 5600.00m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20240530-020",
                PolicyNumber = "POL-345678901",
                ClaimantName = "David Jones",
                ClaimantContact = "david.jones@email.com",
                DateOfAccident = new DateTime(2024, 5, 30, 19, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "BMW",
                VehicleModel = "3 Series",
                VehicleYear = 2024,
                LicensePlate = "MNO6789",
                AmountClaimed = 1275.25m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240615-021",
                PolicyNumber = "POL-345678901",
                ClaimantName = "David Jones",
                ClaimantContact = "david.jones@email.com",
                DateOfAccident = new DateTime(2024, 6, 15, 13, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "BMW",
                VehicleModel = "3 Series",
                VehicleYear = 2024,
                LicensePlate = "MNO6789",
                AmountClaimed = 8950.00m,
                Status = ClaimStatus.Approved
            }
        ],
        ["23-456-7890"] = [
            new Claim
            {
                ClaimId = "CLM-20240205-022",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 2, 5, 8, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 4200.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240310-023",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 3, 10, 14, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 2850.50m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240420-024",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 4, 20, 10, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 11750.25m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20240525-025",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 5, 25, 16, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 6400.00m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240610-026",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 6, 10, 12, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 1650.75m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20240705-027",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 7, 5, 18, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 1420.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240815-028",
                PolicyNumber = "POL-567890123",
                ClaimantName = "Emily Garcia",
                ClaimantContact = "emily.garcia@gmail.com",
                DateOfAccident = new DateTime(2024, 8, 15, 7, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Mercedes-Benz",
                VehicleModel = "C-Class",
                VehicleYear = 2023,
                LicensePlate = "PQR7890",
                AmountClaimed = 7800.25m,
                Status = ClaimStatus.Closed
            }
        ],
        ["34-567-8901"] = [
            new Claim
            {
                ClaimId = "CLM-20240120-029",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 1, 20, 15, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 4800.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240228-030",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 2, 28, 9, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 1950.50m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20240405-031",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 4, 5, 17, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 2300.25m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20240510-032",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 5, 10, 11, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 9600.75m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240620-033",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 6, 20, 13, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 2750.00m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240725-034",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 7, 25, 8, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 1180.50m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240830-035",
                PolicyNumber = "POL-678901234",
                ClaimantName = "Robert Miller",
                ClaimantContact = "robert.miller@outlook.com",
                DateOfAccident = new DateTime(2024, 8, 30, 14, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Audi",
                VehicleModel = "A4",
                VehicleYear = 2022,
                LicensePlate = "STU8901",
                AmountClaimed = 6200.25m,
                Status = ClaimStatus.UnderReview
            }
        ],
        ["56-789-0123"] = [
            new Claim
            {
                ClaimId = "CLM-20231015-036",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2023, 10, 15, 12, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 850.00m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20231201-037",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2023, 12, 1, 16, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 4500.75m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240115-038",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2024, 1, 15, 10, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 1900.50m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240305-039",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2024, 3, 5, 14, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 1650.25m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20240418-040",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2024, 4, 18, 18, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 7200.00m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20240520-041",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2024, 5, 20, 7, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 1200.75m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240625-042",
                PolicyNumber = "POL-890123456",
                ClaimantName = "Jennifer Davis",
                ClaimantContact = "jennifer.davis@yahoo.com",
                DateOfAccident = new DateTime(2024, 6, 25, 15, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleYear = 2019,
                LicensePlate = "VWX0123",
                AmountClaimed = 3800.50m,
                Status = ClaimStatus.Closed
            }
        ],
        ["78-901-2345"] = [
            new Claim
            {
                ClaimId = "CLM-20240312-043",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 3, 12, 11, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 8900.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240415-044",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 4, 15, 9, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 1100.25m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20240518-045",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 5, 18, 13, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 1800.50m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20240622-046",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 6, 22, 17, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 2200.00m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20240728-047",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 7, 28, 8, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 4100.75m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240830-048",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 8, 30, 12, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 975.25m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240915-049",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 9, 15, 16, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 5600.50m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20241020-050",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 10, 20, 14, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Collision at intersection. Front-end damage.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 10200.00m,
                Status = ClaimStatus.Submitted
            },
            new Claim
            {
                ClaimId = "CLM-20241105-051",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 11, 5, 10, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Side-swiped in parking lot. Scratches along driver side door.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 1750.75m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20241210-052",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 12, 10, 18, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Vandalism - keyed along passenger side.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 1320.00m,
                Status = ClaimStatus.Rejected
            },
            new Claim
            {
                ClaimId = "CLM-20241215-053",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 12, 15, 6, 30, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 2450.50m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20241220-054",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 12, 20, 15, 15, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 1050.25m,
                Status = ClaimStatus.UnderReview
            },
            new Claim
            {
                ClaimId = "CLM-20241222-055",
                PolicyNumber = "POL-012345678",
                ClaimantName = "William Anderson",
                ClaimantContact = "w.anderson@email.com",
                DateOfAccident = new DateTime(2024, 12, 22, 19, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit by falling tree branch during storm. Dent in roof.",
                VehicleMake = "Honda",
                VehicleModel = "Accord",
                VehicleYear = 2020,
                LicensePlate = "YZA2345",
                AmountClaimed = 3950.00m,
                Status = ClaimStatus.Submitted
            }
        ],
        ["90-123-4567"] = [
            new Claim
            {
                ClaimId = "CLM-20240108-056",
                PolicyNumber = "POL-234567890",
                ClaimantName = "Lisa Thompson",
                ClaimantContact = "lisa.thompson@gmail.com",
                DateOfAccident = new DateTime(2024, 1, 8, 13, 20, 0, DateTimeKind.Utc),
                AccidentDescription = "Hit-and-run incident. Damage to front quarter panel.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2021,
                LicensePlate = "BCD4567",
                AmountClaimed = 6500.00m,
                Status = ClaimStatus.Approved
            },
            new Claim
            {
                ClaimId = "CLM-20240222-057",
                PolicyNumber = "POL-234567890",
                ClaimantName = "Lisa Thompson",
                ClaimantContact = "lisa.thompson@gmail.com",
                DateOfAccident = new DateTime(2024, 2, 22, 8, 45, 0, DateTimeKind.Utc),
                AccidentDescription = "Minor fender bender in parking garage.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2021,
                LicensePlate = "BCD4567",
                AmountClaimed = 1350.50m,
                Status = ClaimStatus.Closed
            },
            new Claim
            {
                ClaimId = "CLM-20240405-058",
                PolicyNumber = "POL-234567890",
                ClaimantName = "Lisa Thompson",
                ClaimantContact = "lisa.thompson@gmail.com",
                DateOfAccident = new DateTime(2024, 4, 5, 16, 0, 0, DateTimeKind.Utc),
                AccidentDescription = "Rear-ended at a stoplight. Minor damage to rear bumper.",
                VehicleMake = "Ford",
                VehicleModel = "F-150",
                VehicleYear = 2021,
                LicensePlate = "BCD4567",
                AmountClaimed = 2800.25m,
                Status = ClaimStatus.UnderReview
            }
        ]
    };

    public Task<IEnumerable<Claim>> GetClaimsHistoryAsync(string customerId)
    {
        // Return hard-coded claims for the specified customer ID
        if (CustomerClaims.TryGetValue(customerId, out var claims))
        {
            // Sort by claim date descending (most recent first)
            var sortedClaims = claims.OrderByDescending(c => c.DateOfAccident);
            return Task.FromResult<IEnumerable<Claim>>(sortedClaims);
        }

        // Return empty list for unknown customer IDs
        return Task.FromResult<IEnumerable<Claim>>([]);
    }
}