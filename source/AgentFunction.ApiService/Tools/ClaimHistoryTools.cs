using System.ComponentModel;
using AgentFunction.ApiService.Services;
using AgentFunction.Models;
using ModelContextProtocol.Server;

namespace AgentFunction.ApiService.Tools;

[McpServerToolType]
public class ClaimHistoryTools(IClaimsService claimsService)
{
    [McpServerTool(Name = "GetClaimsHistory"), Description("Retrieves the claims history for a specific customer.")]
    public Task<IEnumerable<Claim>> GetClaimsHistoryAsync(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        }

        return claimsService.GetClaimsHistoryAsync(customerId);
    }
}