using System.Text.Json.Serialization;

namespace Shared.Models
{
    // -------------------------------------------------------
    // FNOL (raw intake) models
    // -------------------------------------------------------
    public record FnolClaim(
        string ClaimId,
        string PolicyId,
        DateTimeOffset LossDate,
        string Vehicle,                 // raw free-text vehicle desc
        string Location,                // raw free-text location
        string Description,
        Party[] Parties,
        Attachment[] Attachments);

    public record Party(string Role, string Name, Contact? Contact = null);

    public record Contact(string? Phone, string? Email);

    public record Attachment(string Type, string Format, string Uri);

    // -------------------------------------------------------
    // Canonicalized models (normalized for downstream agents)
    // -------------------------------------------------------
    public record CanonicalClaim(
        string ClaimId,
        string PolicyId,
        DateTimeOffset LossDate,
        VehicleInfo Vehicle,
        AddressInfo Location,
        string Description,
        CanonicalParty[] Parties);

    public record VehicleInfo(
        string Make,
        string Model,
        string? Trim,
        int? Year,
        string? Vin);

    public record AddressInfo(
        string Line1,
        string City,
        string State,
        string PostalCode);

    public record CanonicalParty(
        PartyRole Role,
        string Name,
        Contact? Contact = null,
        AddressInfo? Address = null);

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PartyRole
    {
        Insured,
        ThirdParty,
        Witness,
        Claimant
    }

    // -------------------------------------------------------
    // Completeness Agent output
    // -------------------------------------------------------
    public record CompletenessResult(
        string[] MissingFields,          // JSON Pointer paths
        string[] ClarifyingQuestions);   // human-friendly questions

    // -------------------------------------------------------
    // Coverage Agent output
    // -------------------------------------------------------
    public record CoverageResult(
        bool Covered,
        CoverageBasis[] Basis,
        string Notes,
        decimal? Deductible = null,
        decimal? CoverageLimit = null);

    public record CoverageBasis(
        string Section,   // e.g. "Collision Coverage §2.1"
        string Reason);   // justification tied to that section

    // -------------------------------------------------------
    // Fraud Agent output (example)
    // -------------------------------------------------------
    public record FraudResult(
        double Score,     // 0-1 risk score
        string[] Signals);

    // -------------------------------------------------------
    // Timeline Agent output (example)
    // -------------------------------------------------------
    public record Timeline(
        TimelineEvent[] Events);

    public record TimelineEvent(
        DateTimeOffset When,
        string Who,
        string What);

    // -------------------------------------------------------
    // Aggregate report (persisted)
    // -------------------------------------------------------
    public class ClaimAnalysisReport
    {
        public string ClaimId { get; init; } = default!;
        public FnolClaim Raw { get; init; } = default!;
        public CompletenessResult Completeness { get; init; } =
            new(Array.Empty<string>(), Array.Empty<string>());
        public CanonicalClaim Canonical { get; init; } = default!;
        public CoverageResult Coverage { get; init; } =
            new(false, Array.Empty<CoverageBasis>(), "");
        public FraudResult Fraud { get; init; } =
            new(0, Array.Empty<string>());
        public Timeline Timeline { get; init; } =
            new(Array.Empty<TimelineEvent>());
    }
}
