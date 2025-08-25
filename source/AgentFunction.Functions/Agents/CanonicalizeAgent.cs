using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AgentFunction.Functions.Agents;

public sealed class CanonicalizeAgent : AgentBase<FnolClaim, CanonicalClaim>
{
    private readonly ILogger<CanonicalizeAgent> _typedLogger;

    public CanonicalizeAgent(Kernel kernel, ILogger<CanonicalizeAgent> logger)
        : base(kernel,
               logger,
               name: "CanonicalizeAgent",
               instructions: @"You are an agent that canonicalizes insurance claims.
            
            Goal:
            - Convert a raw FNOL JSON payload into a standardized CanonicalClaim format.
            - Ensure all required fields are present and correctly formatted.

            ### Canonical target JSON schema (shape only):
            {
                ""ClaimId"": string,
                ""PolicyId"": string,
                ""LossDate"": string (ISO-8601, UTC),
                ""Vehicle"": { ""Make"": string, ""Model"": string, ""Trim"": string|null, ""Year"": number|null, ""Vin"": string|null },
                ""Location"": { ""Line1"": string, ""City"": string, ""State"": string, ""PostalCode"": string },
                ""Description"": string,
                ""Parties"": [ { ""Role"": ""Insured|ThirdParty|Witness|Claimant"", ""Name"": string,
                                ""Contact"": { ""Phone"": string|null, ""Email"": string|null } | null,
                                ""Address"": { ""Line1"": string, ""City"": string, ""State"": string, ""PostalCode"": string } | null } ]
            }

            ### Rules
            - Parse free-text vehicle (e.g., ""2019 Honda Civic LX"") into fields.
            - If year is present, set Vehicle.Year; else null.
            - Keep VIN null unless explicitly provided/known.
            - Split free-text location into AddressInfo (Line1, City, State 2-letter, PostalCode 5-digit if present).
            - Normalize party roles to one of: Insured, ThirdParty, Witness, Claimant.
            - Preserve Description verbatim.
            - Prefer precision; if unsure, leave a field null rather than guessing.

            ### Output
            Return **ONLY** the CanonicalClaim as strict JSON with the exact property names above. No markdown, no commentary."
               )
    {
        _typedLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<CanonicalClaim> ProcessAsync(FnolClaim input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        _typedLogger.LogInformation("CanonicalizeAgent: starting processing for claim {ClaimId}", input.ClaimId);

        var fnolJson = SerializeInput(input);

        var userMessage = new ChatMessageContent(
            role: AuthorRole.User,
            content: $"Canonicalize this FNOL into CanonicalClaim JSON as per instructions.\nRAW FNOL:\n```json\n{fnolJson}\n```"
        );

        var canonical = await InvokeAndDeserializeAsync<CanonicalClaim>(
            userMessage,
            cancellationToken: ct).ConfigureAwait(false);

        if (canonical is null)
        {
            _typedLogger.LogWarning("CanonicalizeAgent: parsed canonical claim was null; returning a minimal fallback.");
            // Return a minimal fallback to keep downstream code simpler; fields set to defaults where possible
            return new CanonicalClaim(input.ClaimId,
                                      input.PolicyId,
                                      input.LossDate,
                                      new VehicleInfo("", "", null, null, null),
                                      new AddressInfo("", "", "", ""),
                                      input.Description ?? "",
                                      Array.Empty<CanonicalParty>());
        }

        return canonical;
    }
}