using AgentFunction.Models;

namespace AgentFunction.Models.Dtos;

/// <summary>
/// Data transfer object representing a simplified view of a claim for API responses.
/// </summary>
public record ClaimDto
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
    /// Make and model of the vehicle involved.
    /// </summary>
    public string? Vehicle { get; init; }

    /// <summary>
    /// Year of the vehicle involved.
    /// </summary>
    public int VehicleYear { get; init; }

    /// <summary>
    /// Amount claimed for the accident.
    /// </summary>
    public decimal AmountClaimed { get; init; }

    /// <summary>
    /// Status of the claim.
    /// </summary>
    public ClaimStatus Status { get; init; }
}