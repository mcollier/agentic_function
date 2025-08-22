using AgentFunction.Functions.Activities;
using AgentFunction.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions;

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
        var completenessResult = await context.CallActivityAsync<CompletenessResult>(nameof(CompletenessActivity.RunCompletnessAssessment), fnolClaim);

        if (completenessResult.MissingFields.Length == 0)
        {
            logger.LogInformation("FNOL claim is complete. Proceeding with processing.");
        }
        else
        {
            logger.LogWarning("FNOL claim is incomplete. Missing fields: {MissingFields}", string.Join(", ", completenessResult.MissingFields));
            // Handle incomplete claim logic here, e.g., notify user or halt processing
            return new ClaimAnalysisReport
            {
                ClaimId = fnolClaim.ClaimId ?? "unknown",
                Raw = fnolClaim,
                Completeness = completenessResult,
                Canonical = null, // No canonicalization if incomplete
                Coverage = null,
                Fraud = null,
                Timeline = null
            };
        }
        // 2) Canonicalize RAW → Canonical
        var canonical = await context.CallActivityAsync<CanonicalClaim>(nameof(CanonicalizeActivity.RunCanonicalize), fnolClaim);

        // 3) Fan-out on CANONICAL
        var coverageTask = context.CallActivityAsync<CoverageResult>(nameof(CoverageActivity.RunCoverage), canonical);
        //     var fraudTask    = context.CallActivityAsync<FraudResult>("RunFraud", canonical);
        //     var timelineTask = context.CallActivityAsync<Timeline>("BuildTimeline", fnol); // or canonical

        await Task.WhenAll(coverageTask);

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