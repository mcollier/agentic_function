using AgentFunction.Functions;

using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

using Shared.Models;

public static class ClaimOrchestrator
{
    [Function(nameof(RunClaimOrchestration))]
    public static async Task<ClaimAnalysisReport> RunClaimOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(RunClaimOrchestration));
        logger.LogInformation("Starting claim processing orchestration.");

        FnolClaim fnolClaim = context.GetInput<FnolClaim>()
            ?? throw new ArgumentNullException(nameof(context), "No claim data provided to orchestration.");

        // 1 - Completeness check over Raw FNOL
        var completenessResult = await context.CallActivityAsync<CompletenessResult>(nameof(RunCompleteness.RunCompletnessAssessment), fnolClaim);

        // 3) Fan-out on CANONICAL
        //     var coverageTask = ctx.CallActivityAsync<CoverageResult>("RunCoverage", canonical);
        //     var fraudTask    = ctx.CallActivityAsync<FraudResult>("RunFraud", canonical);
        //     var timelineTask = ctx.CallActivityAsync<Timeline>("BuildTimeline", fnol); // or canonical

        //     await Task.WhenAll(coverageTask, fraudTask, timelineTask);

        //     // 4) Aggregate & Comms
        //     var report = new ClaimAnalysisReport
        //     {
        //         ClaimId = fnol.ClaimId,
        //         Raw = fnol,
        //         Completeness = completeness,
        //         Canonical = canonical,
        //         Coverage = coverageTask.Result,
        //         Fraud = fraudTask.Result,
        //         Timeline = timelineTask.Result
        //     };

        //     var comms = await ctx.CallActivityAsync<CommsOutput>("GenerateComms", report);
        //     await ctx.CallActivityAsync("FinalizeClaim", (report, comms)); // see note below

        //     return report;

        return null;
    
    // return new ClaimAnalysisReport
        //     {
        //         ClaimId = fnolClaim.ClaimId ?? "unknown",
        //         Raw = fnolClaim,
        //         Completeness = completenessResult,
        //         Canonical = new CanonicalClaim(fnolClaim.ClaimId, fnolClaim.PolicyId, ), // placeholder
        //         Coverage = new CoverageResult(false, Array.Empty<CoverageBasis>(), ""),
        //         Fraud = new FraudResult(0, Array.Empty<string>()),
        //         Timeline = new Timeline(Array.Empty<TimelineEvent>())
        //     };
    }
}