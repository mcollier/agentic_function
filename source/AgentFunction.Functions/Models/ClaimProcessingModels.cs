using AgentFunction.Models;

namespace AgentFunction.Functions.Models;

public record ClaimCompletionResult (
    string ClaimId,
    bool IsComplete,
    string Message
);

public record ClaimFraudRequest(
    Claim Claim,
    ClaimHistory History
);

public record ClaimFraudResult(
    string ClaimId = "",
    bool IsFraudulent = false,
    string Reason = "",
    int Confidence = 0
);

public record ClaimHistory (
    string ClaimId = "",
    List<Claim> ClaimHistoryItems = null
);
