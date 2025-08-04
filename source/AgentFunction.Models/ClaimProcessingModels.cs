namespace AgentFunction.Models;

public record ClaimCompletionResult(
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
    string CustomerId = "",
    int TotalClaims = 0,
    decimal TotalClaimAmount = 0.0m,
    DateTime MostRecentClaimDate = default
);

public record ClaimSummaryResult(
    string ClaimId = "",
    string Summary = ""
);

public record NotificationRequest(
    string ClaimId,
    string EmailAddress,
    string EmailBody
);