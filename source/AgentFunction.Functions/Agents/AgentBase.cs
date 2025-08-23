using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

public abstract class AgentBase<TInput, TOutput> : IAgent<TInput, TOutput>
{
    protected readonly ChatCompletionAgent _agent;
    protected readonly ILogger _logger;
    protected readonly Kernel _kernel;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected AgentBase(Kernel kernel, ILogger logger,
                        string name, string instructions, KernelArguments? arguments = null)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        _agent = new ChatCompletionAgent()
        {
            Name = name,
            Instructions = instructions,
            Kernel = kernel,
            Arguments = arguments ?? new KernelArguments(new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };
    }

    public abstract Task<TOutput> ProcessAsync(TInput input, CancellationToken ct = default);

    protected string SerializeInput<T>(T input, JsonSerializerOptions? opts = null)
    {
        var options = opts ?? _jsonOptions;
        return JsonSerializer.Serialize(input, options);
    }

    protected record AgentResponse(string? RawContent, object? Usage, ChatMessageContent? Message);

    protected async Task<AgentResponse> InvokeAgentAsync(ChatMessageContent message, OpenAIPromptExecutionSettings? execSettings = null, CancellationToken ct = default)
    {
        var settings = execSettings ?? new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var options = new AgentInvokeOptions()
        {
            KernelArguments = new KernelArguments(settings),
            Kernel = _kernel
        };

        ChatMessageContent? chatMessageContent = null;
        try
        {
            IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> response =
                _agent.InvokeAsync(message: message,
                                   options: options,
                                   cancellationToken: ct);

            await foreach (AgentResponseItem<ChatMessageContent> item in response.ConfigureAwait(false))
            {
                chatMessageContent = item.Message;
                break;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Agent invocation cancelled.");
            return new AgentResponse(null, null, null);
        }

        object? usage = null;
        if (chatMessageContent?.Metadata != null && chatMessageContent.Metadata.TryGetValue("Usage", out var u))
        {
            usage = u;
        }

        return new AgentResponse(chatMessageContent?.Content, usage, chatMessageContent);
    }

    protected async Task<TResult?> InvokeAndDeserializeAsync<TResult>(ChatMessageContent message, Func<string, TResult?>? customDeserializer = null,
                                    OpenAIPromptExecutionSettings? execSettings = null,
                                    CancellationToken ct = default)
    {
        var resp = await InvokeAgentAsync(message, execSettings, ct).ConfigureAwait(false);
        OnRawResponse(resp.RawContent, resp.Usage);

        var raw = resp.RawContent;
        if (string.IsNullOrWhiteSpace(raw))
        {
            _logger.LogWarning("Agent returned empty response.");
            return default;
        }

        try
        {
            if (customDeserializer is not null)
            {
                return customDeserializer(raw);
            }

            return JsonSerializer.Deserialize<TResult>(raw, _jsonOptions);
        }
        catch (JsonException je)
        {
            _logger.LogError(je, "Failed to deserialize agent response. Raw: {Raw}", raw);
            return default;
        }
    }

    protected virtual void OnRawResponse(string? raw, object? usage)
    {
        _logger.LogInformation("Agent Response: {Response}", raw ?? "<null>");
    }
}
