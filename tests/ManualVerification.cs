using AgentFunction.ApiService.Services;
using AgentFunction.Models;

namespace AgentFunction.Tests.Manual;

/// <summary>
/// Manual verification tests to check the ClaimHistoryService implementation
/// since we can't run automated tests due to .NET 9 SDK requirement.
/// </summary>
public static class ManualVerification
{
    public static async Task VerifyClaimHistoryService()
    {
        var service = new ClaimHistoryService();
        
        Console.WriteLine("=== Manual Verification of ClaimHistoryService ===");
        
        // Test 1: Valid policy with multiple claims
        Console.WriteLine("\n1. Testing POL-555555555 (should return 3 claims):");
        var claims1 = await service.GetClaimsByPolicyIdAsync("POL-555555555");
        Console.WriteLine($"   Found {claims1.Count()} claims");
        foreach (var claim in claims1)
        {
            Console.WriteLine($"   - {claim.ClaimId}: {claim.AccidentDescription} (${claim.AmountClaimed})");
        }
        
        // Test 2: Case insensitive
        Console.WriteLine("\n2. Testing pol-123456789 (case insensitive, should return 2 claims):");
        var claims2 = await service.GetClaimsByPolicyIdAsync("pol-123456789");
        Console.WriteLine($"   Found {claims2.Count()} claims");
        foreach (var claim in claims2)
        {
            Console.WriteLine($"   - {claim.ClaimId}: {claim.AccidentDescription} (${claim.AmountClaimed})");
        }
        
        // Test 3: Non-existent policy
        Console.WriteLine("\n3. Testing POL-999999999 (non-existent, should return 0 claims):");
        var claims3 = await service.GetClaimsByPolicyIdAsync("POL-999999999");
        Console.WriteLine($"   Found {claims3.Count()} claims");
        
        // Test 4: Null/empty policy
        Console.WriteLine("\n4. Testing empty policy ID (should return 0 claims):");
        var claims4 = await service.GetClaimsByPolicyIdAsync("");
        Console.WriteLine($"   Found {claims4.Count()} claims");
        
        Console.WriteLine("\n=== Verification Complete ===");
    }
}

// Uncomment to run verification:
// await ManualVerification.VerifyClaimHistoryService();