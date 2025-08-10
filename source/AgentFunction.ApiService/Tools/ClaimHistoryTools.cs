using System.ComponentModel;
using AgentFunction.ApiService.Services;
using AgentFunction.Models;
using ModelContextProtocol.Server;

namespace AgentFunction.ApiService.Tools;

[McpServerToolType]
public class ClaimHistoryTools(IClaimsService claimsService)
{
    [McpServerTool(Name = "get_claims_history"), Description("Retrieves the claims history for a specific customer.")]
    public async Task<IEnumerable<ClaimDetail>> GetClaimsHistoryAsync(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        }

        var claimHistory = await claimsService.GetClaimsHistoryAsync(customerId);

        Console.WriteLine($"GetClaimsHistoryAsync called for customerId: {customerId}, returning {claimHistory?.Count() ?? 0} claims.");
        Console.WriteLine(claimHistory);

        return claimHistory ?? [];
    }
}