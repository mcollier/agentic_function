using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel;

namespace AgentFunction.Functions.Plugins;

public sealed partial class PolicyTools
{
    // [KernelFunction("get_policy_details_by_id")]
    // [Description("Get policy details by policy ID.")]
    // public static string GetPolicyDetailsById(
    //     [Description(@"Exact policy identifier (e.g., ""P-998877""). Must match ^P-\d{5,10}$.")] string policyId)
    // {
    //     string policyText = """
    //                         Collision Coverage §2.1
    //                         We cover direct and accidental loss to your covered auto caused by collision...
    //                         Deductible §2.4
    //                         A $500 deductible applies to each collision claim...
    //                         Exclusions §3.2
    //                         We do not cover losses that occur during commercial use...
    //                         """; // This should be provided or fetched from a relevant source

    //     return policyText;
    // }

    [KernelFunction("get_policy_details_by_id")]
    // [Description("Get policy details by policy ID.")]
    [Description(
        @"Fetch the full policy text for a specific policy ID.

        WHEN TO CALL
        - You already have a concrete policyId (e.g., ""P-998877"") and need the exact policy wording to analyze coverage.
        - Call at most ONCE per distinct policyId in a conversation turn; reuse the retrieved text for citations.

        WHEN NOT TO CALL
        - You only have a name, claimId, or partial/guessed ID.
        - You need a summary (summarize the returned text yourself).
        - You are unsure which policy applies (ask the user to clarify instead of calling).

        INPUT
        - policyId: Exact policy identifier. Must match ^P-\d{5,10}$. Do not pass names, claim numbers, or fuzzy strings.

        OUTPUT
        - Returns the raw policy document as UTF‑8 text (Markdown format). No additional commentary.

        FAILURE / EDGE CASES
        - If the file is missing or the ID is invalid, the function throws with a clear message. Do not retry with the same invalid ID.
        "
    )]
    public static Task<string> GetPolicyDetailsByIdAsync(
        [Description(@"Exact policy identifier (e.g., ""P-998877""). Must match ^P-\d{5,10}$.")] string policyId)
    {
        if (string.IsNullOrWhiteSpace(policyId) || !MyRegex().IsMatch(policyId))
        {
            throw new ArgumentException("Invalid policyId format. Must match ^P-\\d{5,10}$.", nameof(policyId));
        }
        
        // Implementation goes here
        var path = $"samples/policies/{policyId}.md";

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Policy document not found for policyId: {policyId}", path);
        }

        return File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^P-\d{5,10}$")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}