using System.ComponentModel;

using AgentFunction.ApiService.Models;
using AgentFunction.ApiService.Services;

using ModelContextProtocol.Server;

namespace AgentFunction.ApiService.Tools;

[McpServerToolType]
public class ClaimHistoryTools(IClaimHistoryService claimHistoryService)
{
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