using AgentFunction.Functions.Agents;
using AgentFunction.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions.Activities;

public class CompletenessActivity(CompletenessAgent completenessAgent)
{
    [Function(nameof(RunCompletenessAssessment))]
    public async Task<CompletenessResult> RunCompletenessAssessment(
        [ActivityTrigger] FnolClaim claim,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(RunCompletenessAssessment));

        return await completenessAgent.ProcessAsync(claim);
    }
}
