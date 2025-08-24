using AgentFunction.Models.Dtos;

namespace AgentFunction.ApiService.Services;

/// <summary>
/// Service interface for retrieving claim history by policy ID.
/// </summary>
public interface IClaimHistoryService
{
    /// <summary>
    /// Retrieves all claims associated with a specific policy ID.
    /// </summary>
    /// <param name="policyId">The policy ID to search for (case-insensitive)</param>
    /// <returns>A collection of claims for the specified policy. Returns empty collection if no matches found.</returns>
    Task<IEnumerable<ClaimDto>> GetClaimsByPolicyIdAsync(string policyId);
}