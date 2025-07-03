namespace AgentFunction.ApiService.Models;

public record Claim(
    string ClaimId,
    string PolicyNumber,
    string ClaimantName,
    string ClaimantContact,
    DateTime DateOfAccident,
    string AccidentDescription,
    string VehicleMake,
    string VehicleModel,
    int VehicleYear,
    string LicensePlate,
    decimal AmountClaimed,
    string Status
);