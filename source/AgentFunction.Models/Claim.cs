namespace AgentFunction.Models;

public record Customer
{
    /// <summary>
    /// Unique identifier for the customer.
    /// </summary>
    public required string CustomerId { get; init; }

    /// <summary>
    /// Name of the customer.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Contact information for the customer.
    /// </summary>
    public string? ContactInfo { get; init; }
}

public record ClaimDetail
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
    /// Date and time of the accident.
    /// </summary>
    public DateTime DateOfAccident { get; init; }

    /// <summary>
    /// Description of the accident.
    /// </summary>
    public string? AccidentDescription { get; init; }

    /// <summary>
    /// Make of the vehicle involved.
    /// </summary>
    public string? VehicleMake { get; init; }

    /// <summary>
    /// Model of the vehicle involved.
    /// </summary>
    public string? VehicleModel { get; init; }

    /// <summary>
    /// Year of the vehicle involved.
    /// </summary>
    public int VehicleYear { get; init; }

    /// <summary>
    /// License plate number of the vehicle.
    /// </summary>
    public string? LicensePlate { get; init; }

    /// <summary>
    /// Amount claimed for the accident.
    /// </summary>
    public decimal AmountClaimed { get; init; }

    /// <summary>
    /// Status of the claim.
    /// </summary>
    public ClaimStatus Status { get; set; }
}

public record Claim
{
    public required Customer Customer { get; init; }

    public required ClaimDetail ClaimDetail { get; set; }

}

public record ClaimHistory
{
    public required Customer Customer { get; init; }
    
    public required IEnumerable<ClaimDetail> Claims { get; init; }
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