namespace AgentFunction.Functions;

public interface IAgent<TIn, TOut>
{
    Task<TOut> ExecuteAsync(TIn input, CancellationToken ct = default);
}
