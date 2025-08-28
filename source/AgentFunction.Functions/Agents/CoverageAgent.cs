using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.Functions.Agents;

public sealed class CoverageAgent(Kernel kernel, ILogger<CoverageAgent> logger) :
                    AgentBase<CanonicalClaim, CoverageResult>(
            kernel,
            logger,
            name: "CoverageAgent",
            template: "Resources/Coverage.yaml",
            templateName: "Coverage"
               )
{
    private readonly ILogger<CoverageAgent> _typedLogger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<CoverageResult> ProcessAsync(CanonicalClaim input, CancellationToken ct = default)
    {
        var claimJson = SerializeInput(input);

        var userMessage = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Analyze this claim JSON to determine if the claim is covered by the policy.\n" +
                     $"Look up the policy details using the policy ID." +
                     $"Claim JSON: {claimJson}\n"
        );

        var coverage = await InvokeAndDeserializeAsync<CoverageResult>(
            userMessage,
            cancellationToken: ct).ConfigureAwait(false);

        return coverage ?? new CoverageResult(0, false, [], "");
    }
}