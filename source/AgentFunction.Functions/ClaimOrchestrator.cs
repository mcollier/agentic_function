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
        var completenessResult = await context.CallActivityAsync<CompletenessResult>(nameof(CompletenessActivity.RunCompletenessAssessment), fnolClaim);

        if (completenessResult.MissingFields.Count == 0)
        {
            logger.LogInformation("FNOL claim is complete. Proceeding with processing.");
        }
        else
        {
            logger.LogWarning("FNOL claim is incomplete. Missing fields: {MissingFields}", string.Join(", ", completenessResult.MissingFields));

            // Handle incomplete claim logic here, e.g., notify user or halt processing
            var incompleteReport = new ClaimAnalysisReport
            {
                ClaimId = fnolClaim.ClaimId ?? "unknown",
                Raw = fnolClaim,
                Completeness = completenessResult,
                Canonical = null!, // No canonicalization if incomplete
                Coverage = null!,
                Fraud = null!,
                Timeline = null!
            };

            await context.CallActivityAsync(nameof(FinalizeActivity.FinalizeClaim), incompleteReport);

            return incompleteReport;
        }

        // 2) Canonicalize RAW → Canonical
        var canonical = await context.CallActivityAsync<CanonicalClaim>(nameof(CanonicalizeActivity.RunCanonicalize), fnolClaim);

        // 3) Fan-out on CANONICAL
        var coverageTask = context.CallActivityAsync<CoverageResult>(nameof(CoverageActivity.RunCoverage), canonical);
        var fraudTask = context.CallActivityAsync<FraudResult>(nameof(FraudActivity.RunFraud), canonical);

        await Task.WhenAll(coverageTask, fraudTask);

        // If the fraud score is above a certain threshold, we might want to halt further processing or flag for review
        if (fraudTask.Result.Score > 0.6)
        {
            logger.LogWarning("High fraud risk detected: {Score}. Further investigation required.", fraudTask.Result.Score);

            using (var timeoutCts = new CancellationTokenSource())
            {
                // Handle high fraud risk logic here, e.g., notify user or escalate
                // DateTime dueTime = context.CurrentUtcDateTime.AddHours(72);
                DateTime dueTime = context.CurrentUtcDateTime.AddMinutes(5); // For demo/testing, use 5 minutes
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                Task<bool> fraudReviewTask = context.WaitForExternalEvent<bool>("FraudReviewCompleted");

                var winner = await Task.WhenAny(fraudReviewTask, durableTimeout);

                if (winner == fraudReviewTask)
                {
                    timeoutCts.Cancel();
                    logger.LogInformation("Fraud review completed. Resuming processing.");
                }
                else
                {
                    // TODO: Do some other activity for when fraud review times out!

                    logger.LogWarning("Fraud review timed out.");
                }
            }
        }
        else
        {
            logger.LogInformation("Fraud risk is within acceptable limits: {Score}.", fraudTask.Result.Score);
        }

        // 4) Aggregate & Comms
        var report = new ClaimAnalysisReport
        {
            ClaimId = fnolClaim.ClaimId,
            Raw = fnolClaim,
            Completeness = completenessResult,
            Canonical = canonical,
            Coverage = coverageTask.Result,
            Fraud = fraudTask.Result,
            Timeline = null! // Haven't implemented timeline processing yet
        };

        // 5) Send communications
        var comms = await context.CallActivityAsync<CommsResult>(nameof(CommunicationActivity.RunComms), report);
        await context.CallActivityAsync(nameof(CommunicationActivity.Send), comms);

        // Include communications in the report
        report.Communications = comms;

        // 6) Finalize - store results
        await context.CallActivityAsync(nameof(FinalizeActivity.FinalizeClaim), report);

        return report;
    }
}