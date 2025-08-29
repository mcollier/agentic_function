using AgentFunction.Functions.Agents;
using AgentFunction.Functions.Models;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions.Activities;

public class FraudActivity(FraudAgent fraudAgent)
{
    [Function(nameof(RunFraud))]
    public async Task<FraudResult> RunFraud(
        [ActivityTrigger] CanonicalClaim claim,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(RunFraud));

        logger.LogInformation("Invoking FraudAgent to process claim {ClaimId}", claim.ClaimId);

        return await fraudAgent.ProcessAsync(claim);
    }
}
