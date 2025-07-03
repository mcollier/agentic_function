using AgentFunction.ApiService.Models;

namespace AgentFunction.ApiService.Services;

public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetClaimsHistoryAsync(string customerId);
}