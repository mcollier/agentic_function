using AgentFunction.Models;

namespace AgentFunction.ApiService.Services;

public interface IClaimsService
{
    Task<IEnumerable<ClaimDetail>> GetClaimsHistoryAsync(string customerId);
}