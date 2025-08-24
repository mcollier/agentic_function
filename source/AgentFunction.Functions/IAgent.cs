namespace AgentFunction.Functions;

public interface IAgent<TIn, TOut>
{
    Task<TOut> ProcessAsync(TIn input, CancellationToken ct = default);
}
