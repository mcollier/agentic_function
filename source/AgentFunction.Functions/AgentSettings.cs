namespace AgentFunction.Functions;

public sealed class AgentSettings
{
    public AgentModel Coverage { get; set; } = new();
    public AgentModel Completeness { get; set; } = new();
    public AgentModel Fraud { get; set; } = new();
    public AgentModel Timeline { get; set; } = new();
    public AgentModel Comms { get; set; } = new();
}

public sealed class AgentModel
{
    public string Provider { get; set; } = "AzureOpenAI"; // or "Foundry", "OpenAI"
    public string ModelId { get; set; } = "";
    public float Temperature { get; set; } = 0.2f;
    public float TopP { get; set; } = 1.0f;
}
