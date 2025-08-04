using System.ComponentModel;
using System.Text.Json;
using AgentFunction.Models;

using Microsoft.SemanticKernel;

namespace AgentFunction.ClaimsAgent.Plugins;

public class ClaimsProcessingPlugin
{
    [KernelFunction("is_claim_complete")]
    [Description("Validates if the claim is complete based on the provided claim data.")]
    public bool IsClaimComplete(string claim)
    {
        Console.WriteLine($"IsClaimComplete called with claim: {claim}");

        // Assume claim is a JSON string; check for required fields
        if (claim is null)
        {
            return false;
        }

        try
        {
            var claimData = JsonSerializer.Deserialize<Claim>(claim);

            bool isComplete = claimData is not null &&
                              !string.IsNullOrWhiteSpace(claimData.ClaimId) &&
                              !string.IsNullOrWhiteSpace(claimData.ClaimantName) &&
                              !string.IsNullOrWhiteSpace(claimData.PolicyNumber) &&
                              claimData.AmountClaimed > 0 &&
                              claimData.DateOfAccident != default;

            Console.WriteLine($"Claim completeness check for {claimData.ClaimId}: {isComplete}");
            return isComplete;
        }
        catch (JsonException ex)
        {
            // Invalid JSON
            Console.WriteLine($"JSON deserialization error: {ex.Message}");
            return false;
        }
    }

    [KernelFunction("is_claim_fraudulent")]
    [Description("Detects if the claim is potentially fraudulent based on claim details and history.")]
    public bool IsClaimFraudulent(string claim, string claimHistory)
    {
        Console.WriteLine($"IsClaimFraudulent called with claim: {claim}");
        Console.WriteLine($"IsClaimFraudulent called with claimHistory: {claimHistory}");

        try
        {
            var claimHistoryItem = JsonSerializer.Deserialize<ClaimHistoryResult>(claimHistory);
            var claimItem = JsonSerializer.Deserialize<Claim>(claim);

            if (claimItem is null || claimHistoryItem is null)
            {
                return false;
            }

            // Simple fraud detection logic
            if (claimItem.AmountClaimed > 10000 && claimHistoryItem.TotalClaims > 5)
            {
                // Example rule: High claim amount with many previous claims
                return true;
            }

            if (claimItem.DateOfAccident > DateTime.Now)
            {
                // Example rule: Accident date is in the future
                return true;
            }

            return false;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON deserialization error: {ex.Message}");
            return false;
        }
    }
}