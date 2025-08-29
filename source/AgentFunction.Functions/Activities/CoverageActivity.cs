using AgentFunction.Functions.Agents;
using AgentFunction.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions.Activities;

public class CoverageActivity(CoverageAgent coverageAgent)
{
    [Function(nameof(RunCoverage))]
    public async Task<CoverageResult> RunCoverage(
        [ActivityTrigger] CanonicalClaim claim,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(RunCoverage));

        return await coverageAgent.ProcessAsync(claim);
    }
}
