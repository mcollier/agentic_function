using AgentFunction.Models;

namespace AgentFunction.Functions.Models;

public record ClaimCompletionResult (
    string ClaimId,
    bool IsComplete,
    string Message
);

public record ClaimFraudRequest(
    Claim Claim,
    ClaimHistoryResult History
);

public record ClaimFraudResult(
    string ClaimId = "",
    bool IsFraudulent = false,
    string Reason = "",
    int Confidence = 0
);

public record ClaimHistoryResult(
    string CustomerId,
    int TotalClaims,
    decimal TotalClaimAmount,
    DateTime MostRecentClaimDate
);
