using System.Text.Json.Serialization;

namespace AgentFunction.ApiService.Models;

/// <summary>
/// Represents a claim with related data required for serialization/deserialization.
/// </summary>
public record ClaimHistory
{
    /// <summary>
    /// Claim identifier from external system.
    /// </summary>
    [JsonPropertyName("claimId")]
    public string ClaimId { get; init; } = string.Empty;

    /// <summary>
    /// Associated policy identifier.
    /// </summary>
    [JsonPropertyName("policyId")]
    public string PolicyId { get; init; } = string.Empty;

    /// <summary>
    /// Date and time the loss occurred (ISO 8601). Use DateTimeOffset to preserve timezone/UTC.
    /// </summary>
    [JsonPropertyName("lossDate")]
    public DateTimeOffset LossDate { get; init; }

    /// <summary>
    /// Date and time the loss was reported.
    /// </summary>
    [JsonPropertyName("reportDate")]
    public DateTimeOffset ReportDate { get; init; }

    /// <summary>
    /// Location where the loss occurred.
    /// </summary>
    [JsonPropertyName("location")]
    public ClaimHistoryLocation Location { get; init; } = new();

    /// <summary>
    /// Vehicle information related to the claim.
    /// </summary>
    [JsonPropertyName("vehicle")]
    public ClaimHistoryVehicle Vehicle { get; init; } = new();

    /// <summary>
    /// Parties involved in the claim (insured, third-party, etc.).
    /// </summary>
    [JsonPropertyName("parties")]
    public List<ClaimHistoryParty> Parties { get; init; } = new();

    /// <summary>
    /// Vendor or repair shop name.
    /// </summary>
    [JsonPropertyName("vendor")]
    public string Vendor { get; init; } = string.Empty;

    /// <summary>
    /// Amount paid or to be paid for the claim. Use decimal for monetary precision.
    /// </summary>
    [JsonPropertyName("payout")]
    public decimal Payout { get; init; }

    /// <summary>
    /// Status of the claim as a string in JSON (Closed, Submitted, Approved, Rejected, etc.).
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClaimStatus Status { get; init; }
}

/// <summary>
/// Postal/location details.
/// </summary>
public record ClaimHistoryLocation
{
    [JsonPropertyName("line1")]
    public string Line1 { get; init; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; init; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; init; } = string.Empty;
}

/// <summary>
/// Vehicle details associated with the claim.
/// </summary>
public record ClaimHistoryVehicle
{
    [JsonPropertyName("make")]
    public string Make { get; init; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("trim")]
    public string? Trim { get; init; }

    [JsonPropertyName("year")]
    public int Year { get; init; }

    [JsonPropertyName("vin")]
    public string? Vin { get; init; }
}

/// <summary>
/// A party involved in the claim (insured, third party, etc.).
/// </summary>
public record ClaimHistoryParty
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("contact")]
    public ClaimHistoryContact Contact { get; init; } = new();
}

/// <summary>
/// Contact information for a party.
/// </summary>
public record ClaimHistoryContact
{
    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }
}

/// <summary>
/// Claim status — serialized/deserialized as string values.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClaimStatus
{
    Unknown,
    Submitted,
    Approved,
    Rejected,
    Closed,
    Open
}