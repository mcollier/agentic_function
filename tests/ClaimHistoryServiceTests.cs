using AgentFunction.ApiService.Services;

using Xunit;

namespace AgentFunction.Tests;


public class ClaimHistoryServiceTests
{
    private readonly ClaimHistoryService _service;

    public ClaimHistoryServiceTests()
    {
        _service = new ClaimHistoryService();
    }

    [Fact(DisplayName = "GetClaimsByPolicyIdAsync with valid policy ID returns matching claims")]
    public async Task GetClaimsByPolicyIdAsync_ValidPolicyId_ReturnsMatchingClaims()
    {
        // Setup
        const string policyId = "P-98765";

        // Execution
        var result = await _service.GetClaimsByPolicyIdAsync(policyId);

        // Verification
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, claim => Assert.Equal(policyId, claim.PolicyId));
    }

    [Fact(DisplayName = "GetClaimsByPolicyIdAsync with case insensitive policy ID returns matching claims")]
    public async Task GetClaimsByPolicyIdAsync_CaseInsensitivePolicyId_ReturnsMatchingClaims()
    {
        // Setup
        const string policyIdLowerCase = "p-98765";
        const string expectedPolicyId = "P-98765";

        // Execution
        var result = await _service.GetClaimsByPolicyIdAsync(policyIdLowerCase);

        // Verification
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, claim => Assert.Equal(expectedPolicyId, claim.PolicyId));
    }

    [Fact(DisplayName = "GetClaimsByPolicyIdAsync with non-existent policy ID returns empty collection")]
    public async Task GetClaimsByPolicyIdAsync_NonExistentPolicyId_ReturnsEmptyCollection()
    {
        // Setup
        const string nonExistentPolicyId = "P-99999";

        // Execution
        var result = await _service.GetClaimsByPolicyIdAsync(nonExistentPolicyId);

        // Verification
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetClaimsByPolicyIdAsync with null policy ID returns empty collection")]
    public async Task GetClaimsByPolicyIdAsync_NullPolicyId_ReturnsEmptyCollection()
    {
        // Execution
        var result = await _service.GetClaimsByPolicyIdAsync(null!);

        // Verification
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetClaimsByPolicyIdAsync with empty policy ID returns empty collection")]
    public async Task GetClaimsByPolicyIdAsync_EmptyPolicyId_ReturnsEmptyCollection()
    {
        // Execution
        var result = await _service.GetClaimsByPolicyIdAsync(string.Empty);

        // Verification
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetClaimsByPolicyIdAsync returns claims ordered by date descending")]
    public async Task GetClaimsByPolicyIdAsync_ValidPolicyId_ReturnsClaimsOrderedByDateDescending()
    {
        // Setup - policy with multiple claims
        const string policyId = "P-98765";

        // Execution
        var result = await _service.GetClaimsByPolicyIdAsync(policyId);

        // Verification
        Assert.NotNull(result);
        var claimsList = result.ToList();
        Assert.True(claimsList.Count > 1);

        for (int i = 0; i < claimsList.Count - 1; i++)
        {
            Assert.True(claimsList[i].LossDate >= claimsList[i + 1].LossDate,
                "Claims should be ordered by loss date descending");
        }
    }
}