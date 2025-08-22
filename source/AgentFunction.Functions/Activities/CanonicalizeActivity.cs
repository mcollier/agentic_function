using AgentFunction.Functions.Agents;
using AgentFunction.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions.Activities;

public class CanonicalizeActivity(CanonicalizeAgent canonicalizeAgent)
{
    [Function(nameof(RunCanonicalize))]
    public async Task<CanonicalClaim> RunCanonicalize(
        [ActivityTrigger] FnolClaim claim,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(RunCanonicalize));

        return await canonicalizeAgent.ExecuteAsync(claim);
    }
}
