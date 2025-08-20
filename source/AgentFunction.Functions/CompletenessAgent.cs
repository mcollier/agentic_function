using AgentFunction.Functions;

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public record CanonicalClaim(
    string ClaimId,
    string PolicyId,
    DateTimeOffset LossDate,
    VehicleInfo Vehicle,
    AddressInfo Location,
    string Description,
    Party[] Parties);

public record Party(string Role, string Name, Contact? Contact = null);

public record Contact(string? Phone, string? Email);
public record VehicleInfo(
    string Make,
    string Model,
    string? Trim,
    int? Year,
    string? Vin);

public record AddressInfo(
    string Line1,
    string City,
    string State,
    string PostalCode);

public record CompletenessResult(
    string[] MissingFields,         // JSON Pointer paths, e.g., "/parties/0/contact/phone"
    string[] ClarifyingQuestions);  // human-friendly questions for the customer
public interface IAgent<TIn, TOut>
{
    Task<TOut> ExecuteAsync(TIn input, CancellationToken ct = default);
}
public sealed class CompletenessAgent : IAgent<CanonicalClaim, CompletenessResult>
{
    private readonly ChatCompletionAgent _agent;
    private readonly AgentModel _cfg;

    public CompletenessAgent(Kernel kernel, IOptions<AgentSettings> settings)
    {
        _cfg = settings.Value.Completeness;

        _agent = new ChatCompletionAgent()
        {
            Name = "CompletenessAgent",
            Instructions = "",
            Kernel = kernel,
            Description = "",
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }
            )
        };
    }

    public Task<CompletenessResult> ExecuteAsync(CanonicalClaim input, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}