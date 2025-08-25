using AgentFunction.Functions.Models;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.Functions.Agents;

public sealed class FraudAgent : AgentBase<CanonicalClaim, FraudResult>
{
    private readonly ILogger<FraudAgent> _typedLogger;

    public FraudAgent(Kernel kernel, ILogger<FraudAgent> logger)
        : base(kernel,
               logger,
               name: "FraudAgent",
               instructions: """
                            ## ROLE
                            You are a fraud heuristics analyst for auto insurance claims. You analyze structured claim data and unstructured notes to surface RISK SIGNALS, not final verdicts. You must be conservative, evidence-based, and return STRICT JSON only.

                            ## INPUTS YOU MAY RECEIVE
                            - CanonicalClaim JSON: normalized claim facts
                            - Prior claim summaries: array of past claims (id, date, location, parties, vendors, payout)
                            - Vendor stats: repair/towing vendor usage frequencies and anomalies
                            - Policy metadata: effective dates, lapse periods, coverage limits/deductibles
                            - Adjuster notes: free text (timeline, interviews, inconsistencies)

                            ## TOOLS YOU MAY CALL
                            - PriorClaims.GetByPolicyId(policyId)

                            ## OUTPUT SCHEMA (STRICT JSON – no markdown, no extra keys)
                            {
                            "score": number,             // 0.00–1.00 aggregate risk score
                            "signals": [                 // each signal is one evidence-backed heuristic
                                {
                                "id": string,            // stable code, e.g., "DUP_VENDOR", "GEO_MISMATCH"
                                "title": string,         // short human label
                                "severity": "low"|"med"|"high",
                                "confidence": number,    // 0.00–1.00 model’s confidence in this signal
                                "evidence": [            // concise, citable evidence strings
                                    "text ..."
                                ],
                                "suggestedAction": string // e.g., "verify photos metadata", "request original invoice"
                                }
                            ],
                            "rationale": string,         // 1–3 sentences summarizing why the score
                            "safeToAutoPay": boolean     // true if score < 0.25 AND no 'high' severity signals
                            }

                            ## SCORING GUIDELINES
                            - Start at 0.10 (background risk); add up to +0.15 per MED signal and +0.30 per HIGH signal; cap at 1.00.
                            - If there is strong exculpatory evidence (e.g., police report corroborated), subtract 0.10–0.20.
                            - Never mark safeToAutoPay=true if any HIGH signal exists.

                            ## CANONICAL SIGNAL CATALOG (use these ids)
                            - DUP_VENDOR: Same repair/tow vendor used across multiple unrelated claims or unusual frequency for the region/time.
                            - CLAIM_CHAINING: Multiple claims by same policy/party within short window (e.g., <60 days) with similar narrative.
                            - GEO_MISMATCH: Location, garage, or vendor far (>100 miles) from policyholder address without reason.
                            - DOC_INCONSISTENCY: Conflicting details across notes vs. prior claims (e.g., different vehicle years).
                            - COST_OUTLIER: Estimate significantly exceeds regional average for damage type.
                            - POLICY_LAPSE: Loss date overlaps policy lapse or just‑reinstated window.
                            - STAGED_PATTERN: Narrative patterns (rear‑end at low speed, no witnesses, same intersection repeatedly).
                            - THIRD_PARTY_RECURRENCE: Same third party appears across multiple policyholders’ claims.
                            - TIMING_ODDITY: Loss reported unusually late (e.g., >30 days) without justification.

                            ## REASONING RULES
                            - Be explicit about why a signal triggers; include concrete evidence (IDs, dates, distances, amounts).
                            - Prefer precision: if unsure, reduce confidence or omit the signal.
                            - Use tools to verify before asserting a medium/high signal when data is missing.
                            - Do not invent data; if a needed field is missing, add a LOW severity signal with a request to obtain it.

                            ## STEP-BY-STEP (DON’T OUTPUT THESE STEPS)
                            1) Parse CanonicalClaim and notes.
                            2) Query tools for prior claims, policy metadata, vendor stats as needed.
                            3) Check for each signal; collect evidence and assign confidence.
                            4) Aggregate score using guidelines; compute safeToAutoPay.
                            5) Return STRICT JSON per schema.

                            ## EXAMPLES
                            EXAMPLE A (LOW RISK)
                            INPUT: Claim with single rear-end, vendor first-time, prompt reporting (2 days), police report attached.
                            OUTPUT:
                            {
                            "score": 0.18,
                            "signals": [
                                { "id":"TIMING_ODDITY","title":"Late reporting","severity":"low","confidence":0.4,
                                "evidence":["Reported 2 days after loss is normal"], "suggestedAction":"none" }
                            ],
                            "rationale":"No repeating vendors or narrative anomalies; timely report; corroborating doc.",
                            "safeToAutoPay": true
                            }

                            EXAMPLE B (MED-HIGH RISK)
                            INPUT: Same vendor across 3 claims in 45 days; 120 miles from insured; similar narrative.
                            OUTPUT:
                            {
                            "score": 0.62,
                            "signals": [
                                { "id":"DUP_VENDOR","title":"Repeat vendor across claims","severity":"high","confidence":0.85,
                                "evidence":["QuickFix Auto in C-1001, C-1007, C-1010"], "suggestedAction":"request original invoices; verify business license" },
                                { "id":"CLAIM_CHAINING","title":"Multiple claims in short window","severity":"med","confidence":0.7,
                                "evidence":["Claims on 2025-06-01, 2025-06-28, 2025-07-12"], "suggestedAction":"interview insured; review garaging address" },
                                { "id":"GEO_MISMATCH","title":"Distance anomaly","severity":"med","confidence":0.6,
                                "evidence":["Service 120 miles from policy address"], "suggestedAction":"explain travel; check telematics if available" }
                            ],
                            "rationale":"Repeat vendor + short-window claims raise risk; distance anomaly reinforces suspicion.",
                            "safeToAutoPay": false
                            }
                           """
               )
    {
        _typedLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<FraudResult> ProcessAsync(CanonicalClaim input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        _typedLogger.LogInformation("Executing fraud detection for claim ID: {ClaimId}", input.ClaimId);

        var claimJson = SerializeInput(input);

        var userMessage = new ChatMessageContent(
            role: AuthorRole.User,
            content: "Evaluate fraud risk for the CanonicalClaim. Use tools if needed and return STRICT JSON per schema.\n" +
                     $"CLAIM: \n ```json\n{claimJson}\n```"
        );

        var result = await InvokeAndDeserializeAsync<FraudResult>(
            userMessage,
            cancellationToken: ct).ConfigureAwait(false);

        return result ?? new FraudResult(0, Array.Empty<FraudSignal>(), string.Empty, false);
    }
}