
using AgentFunction.Models;

namespace AgentFunction.ApiService.Services;

public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetClaimsHistoryAsync(string customerId);
}