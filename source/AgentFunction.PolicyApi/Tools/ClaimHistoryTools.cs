using System.ComponentModel;

using AgentFunction.ApiService.Models;
using AgentFunction.ApiService.Services;

using ModelContextProtocol.Server;

namespace AgentFunction.ApiService.Tools;

[McpServerToolType]
public class ClaimHistoryTools(IClaimHistoryService claimHistoryService)
{
    // [McpServerTool(Name = "get_claims_history"), Description("Retrieves the claims history for a specific customer.")]
    // public async Task<IEnumerable<ClaimDetail>> GetClaimsHistoryAsync(string customerId)
    // {
    //     if (string.IsNullOrWhiteSpace(customerId))
    //     {
    //         throw new ArgumentException("Customer ID is required", nameof(customerId));
    //     }

    //     var claimHistory = await claimsService.GetClaimsHistoryAsync(customerId);

    //     Console.WriteLine($"GetClaimsHistoryAsync called for customerId: {customerId}, returning {claimHistory?.Count() ?? 0} claims.");
    //     Console.WriteLine(claimHistory);

    //     return claimHistory ?? [];
    // }
    [McpServerTool(Name = "get_historical_policy_claims"),
     Description("Retrieves all claims associated with a specific policy ID.")]
    public async Task<IEnumerable<ClaimHistory>> GetPolicyClaimsAsync(string policyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(policyId, nameof(policyId));

        var claims = await claimHistoryService.GetClaimsByPolicyIdAsync(policyId);

        Console.WriteLine($"GetPolicyClaimsAsync called for policyId: {policyId}, returning {claims?.Count() ?? 0} claims.");
        Console.WriteLine(claims);

        return claims ?? [];
    }
}