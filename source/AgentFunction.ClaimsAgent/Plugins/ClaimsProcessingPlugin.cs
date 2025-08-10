using System.ComponentModel;
using System.Text.Json;

using AgentFunction.Models;

using Microsoft.SemanticKernel;

namespace AgentFunction.ClaimsAgent.Plugins;

public class ClaimsProcessingPlugin()
{
    [KernelFunction("is_claim_complete")]
    [Description("""
                 Validates if the claim is complete based on the provided claim data and returns
                 a JSON object with IsComplete (true/false) and a list of MissingFields.
                 Example:
                 {
                     "IsComplete": false,
                     "MissingFields": ["ClaimDetail.ClaimId", "Customer.Name"]
                 }
                 """
                )]
    public ClaimCompletenessResult IsClaimComplete(string claim)
    {
        Console.WriteLine($"IsClaimComplete called with claim: {claim}");

        var missingFields = new List<string>();

        if (claim is null)
        {
            return new ClaimCompletenessResult (IsComplete: false, MissingFields: ["Claim"]);
        }

        try
        {
            var claimData = JsonSerializer.Deserialize<Claim>(claim);

            if (claimData is null)
            {
                missingFields.Add("Claim");
            }
            else
            {
                if (claimData.ClaimDetail is null)
                {
                    missingFields.Add("ClaimDetail");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(claimData.ClaimDetail.ClaimId))
                        missingFields.Add("ClaimDetail.ClaimId");
                    if (string.IsNullOrWhiteSpace(claimData.ClaimDetail.PolicyNumber))
                        missingFields.Add("ClaimDetail.PolicyNumber");
                    if (claimData.ClaimDetail.AmountClaimed <= 0)
                        missingFields.Add("ClaimDetail.AmountClaimed");
                    if (claimData.ClaimDetail.DateOfAccident == default)
                        missingFields.Add("ClaimDetail.DateOfAccident");
                }

                if (claimData.Customer is null)
                {
                    missingFields.Add("Customer");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(claimData.Customer.Name))
                        missingFields.Add("Customer.Name");
                }
            }

            bool isComplete = missingFields.Count == 0;

            var result = new ClaimCompletenessResult (IsComplete: isComplete, MissingFields: missingFields);

            Console.WriteLine($"Claim completeness check: {JsonSerializer.Serialize(result)}");
            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON deserialization error: {ex.Message}");
            return new ClaimCompletenessResult(IsComplete: false,
                MissingFields: ["Invalid JSON"]
            );
        }
    }

    [KernelFunction("is_claim_fraudulent")]
    [Description("""
                Detects if the claim is potentially fraudulent based on claim details and history and returns:
                IsFraudulent: true or false
                Reason: string describing the reason for fraud detection
                Example:
                {
                    "IsFraudulent": true,
                    "Reason": "High claim amount with many previous claims"
                }
                """
                )]
    public FraudDetectionResult IsClaimFraudulent(string claim, string claimHistory)
    {
        Console.WriteLine($"IsClaimFraudulent called with claim: {claim}");
        Console.WriteLine($"IsClaimFraudulent called with claimHistory: {claimHistory}");

        FraudDetectionResult result = new(false, "No fraud detected");

        try
        {
            var claimHistoryItem = JsonSerializer.Deserialize<ClaimHistoryResult>(claimHistory);
            var claimItem = JsonSerializer.Deserialize<Claim>(claim);

            if (claimItem is null || claimHistoryItem is null)
            {
                return new FraudDetectionResult(true, "Invalid claim or claim history data");
            }

            // Simple fraud detection logic
            if (claimItem.ClaimDetail.AmountClaimed > 10000 && claimHistoryItem.TotalClaims > 5)
            {
                // Example rule: High claim amount with many previous claims
                return new FraudDetectionResult(true, "High claim amount with many previous claims");
            }

            if (claimItem.ClaimDetail.DateOfAccident > DateTime.Now)
            {
                // Example rule: Accident date is in the future
                return new FraudDetectionResult(true, "Accident date is in the future");
            }

            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON deserialization error: {ex.Message}");
            return new FraudDetectionResult(true, "Invalid JSON data");
        }
    }
}

public record ClaimCompletenessResult(bool IsComplete, List<string> MissingFields);

public record FraudDetectionResult(bool IsFraudulent, string Reason);