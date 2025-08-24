using System.ComponentModel;
using System.Text;

using Microsoft.SemanticKernel;

namespace AgentFunction.Functions.Plugins;

public sealed class PriorClaimsTools
{
    [KernelFunction("get_prior_claims_by_policy_id")]
    [Description("Get prior claims by policy ID.  Returns a JSON document containing prior claims information.")]
    public static Task<string> GetPriorClaimsByPolicyId(
        [Description(@"Exact policy identifier (e.g., ""P-998877""). Must match ^P-\d{5,10}$.")] string policyId)
    {
        // Implementation goes here
        var path = $"samples/prior-claims/{policyId}.json";
        return File.ReadAllTextAsync(path, Encoding.UTF8);
    }
}
