namespace AgentFunction.Models;

public record Claim
{
    /// <summary>
    /// Unique identifier for the claim.
    /// </summary>
    public required string ClaimId { get; init; }

    /// <summary>
    /// Policy number associated with the claim.
    /// </summary>
    public required string PolicyNumber { get; init; }

    /// <summary>
    /// Name of the claimant.
    /// </summary>
    public required string ClaimantName { get; init; }

    /// <summary>
    /// Contact information for the claimant.
    /// </summary>
    public required string ClaimantContact { get; init; }

    /// <summary>
    /// Date and time of the accident.
    /// </summary>
    public DateTime DateOfAccident { get; init; }

    /// <summary>
    /// Description of the accident.
    /// </summary>
    public required string AccidentDescription { get; init; }

    /// <summary>
    /// Make of the vehicle involved.
    /// </summary>
    public required string VehicleMake { get; init; }

    /// <summary>
    /// Model of the vehicle involved.
    /// </summary>
    public required string VehicleModel { get; init; }

    /// <summary>
    /// Year of the vehicle involved.
    /// </summary>
    public int VehicleYear { get; init; }

    /// <summary>
    /// License plate number of the vehicle.
    /// </summary>
    public required string LicensePlate { get; init; }

    /// <summary>
    /// Amount claimed for the accident.
    /// </summary>
    public decimal AmountClaimed { get; init; }

    /// <summary>
    /// Status of the claim.
    /// </summary>
    public ClaimStatus Status { get; init; }
}

/// <summary>
/// Represents the status of an automobile claim.
/// </summary>
public enum ClaimStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Closed
}