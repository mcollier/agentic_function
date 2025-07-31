using System.ComponentModel;
using System.Text.Json;
using AgentFunction.Models;
using Microsoft.SemanticKernel;

namespace AgentFunction.Functions;

public class ClaimsProcessingPlugin
{
    [KernelFunction("is_claim_complete")]
    [Description("Validates if the claim is complete based on the provided claim data.")]
    public async Task<bool> IsClaimComplete(string claim)
    {
        await Task.Delay(500);

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

            return isComplete;
        }
        catch (JsonException)
        {
            // Invalid JSON
            return false;
        }
    }

    [KernelFunction("is_claim_fraudulent")]
    [Description("Detects if the claim is potentially fraudulent based on claim details and history.")]
    public async Task<bool> IsClaimFraudulent(string claim)
    {
        await Task.Delay(500);

        // Assume claim is a JSON string; check for signs of fraud
        if (claim is null)
        {
            return false;
        }

        try
        {
            return false; // Placeholder: Implement fraud detection logic here
        }
        catch (JsonException)
        {
            // Invalid JSON
            return false;
        }
    }
}