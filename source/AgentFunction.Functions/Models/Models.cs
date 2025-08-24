using System.Text.Json.Serialization;

namespace AgentFunction.Functions.Models
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
    // public record CompletenessResult(
    //     string[] MissingFields,          // JSON Pointer paths
    //     string[] ClarifyingQuestions);   // human-friendly questions

    public record CompletenessResult(
        List<string> MissingFields,          // JSON Pointer paths
        List<string> ClarifyingQuestions);   // human-friendly questions

    // -------------------------------------------------------
    // Coverage Agent output
    // -------------------------------------------------------
    public record CoverageResult(
        double Confidence,
        bool Covered,
        CoverageBasis[] Basis,
        string Notes,
        decimal? Deductible = null,
        decimal? CoverageLimit = null);

    public record CoverageBasis(
        string Section,   // e.g. "Collision Coverage §2.1"
        string Reason);   // justification tied to that section

    // JSON Schema (shape only) for CoverageResult:
    /*
    {
      "type": "object",
      "properties": {
        "Covered": { "type": "boolean" },
        "Basis": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "Section": { "type": "string" },
              "Reason": { "type": "string" }
            },
            "required": ["Section", "Reason"]
          }
        },
        "Notes": { "type": "string" },
        "Deductible": { "type": ["number", "null"] },
        "CoverageLimit": { "type": ["number", "null"] }
      },
      "required": ["Covered", "Basis", "Notes"]
    }
    */

    // -------------------------------------------------------
    // Fraud Agent output (example)
    // -------------------------------------------------------
    public record FraudSignal(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("severity")] string Severity,
        [property: JsonPropertyName("confidence")] double Confidence,
        [property: JsonPropertyName("evidence")] string[] Evidence,
        [property: JsonPropertyName("suggestedAction")] string SuggestedAction);

    public record FraudResult(
        [property: JsonPropertyName("score")] double Score,     // 0-1 risk score
        [property: JsonPropertyName("signals")] FraudSignal[] Signals,
        [property: JsonPropertyName("rationale")] string Rationale,
        [property: JsonPropertyName("safeToAutoPay")] bool SafeToAutoPay);

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
            new([], []);
        public CanonicalClaim Canonical { get; init; } = default!;
        public CoverageResult Coverage { get; init; } =
            new(0, false, [], "", 0, 0);
        public FraudResult Fraud { get; init; } =
            new(0, Array.Empty<FraudSignal>(), string.Empty, false);
        public Timeline Timeline { get; init; } =
            new([]);

        public CommsResult? Communications { get; set; } 
    }

    public record CommsResult(EmailContent Email, string Sms);
    public record EmailContent(string Subject,
                               string Body,
                               string RecipientEmailAddress,
                               string RecipientName);
}
