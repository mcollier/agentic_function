using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentFunction.Functions.Agents;

public abstract class AgentBase<TInput, TOutput> : IAgent<TInput, TOutput>
{
    protected readonly ChatCompletionAgent _agent;
    protected readonly ILogger _logger;
    protected readonly Kernel _kernel;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected AgentBase(Kernel kernel, ILogger logger,
                        string name, string template,
                        string templateName)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentException.ThrowIfNullOrEmpty(template, nameof(template));
        ArgumentException.ThrowIfNullOrEmpty(templateName, nameof(templateName));

        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        if (!File.Exists(template))
        {
            throw new FileNotFoundException($"Template file not found: {template}");
        }

        string templateYaml = File.ReadAllText(template);
        var templateFactory = new KernelPromptTemplateFactory();
        var promptTemplateConfig = new PromptTemplateConfig()
        {
            Template = templateYaml,
            TemplateFormat = "semantic-kernel",
            Name = templateName
        };

        _agent = new(promptTemplateConfig, templateFactory)
        {
            Kernel = kernel,
            Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings()
            {
                ServiceId = "gpt-4o-mini",
                ResponseFormat = "json_object",
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };
    }
    protected AgentBase(Kernel kernel, ILogger logger,
                        string name, string instructions,
                        KernelArguments? arguments = null)
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
            Arguments = arguments ?? new KernelArguments(new AzureOpenAIPromptExecutionSettings()
            {
                ServiceId = "gpt-4o-mini",
                Temperature = 0.2f,
                TopP = 1.0f,
                ResponseFormat = "json_object",
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
        var options = new AgentInvokeOptions()
        {
            Kernel = _kernel,
            KernelArguments = execSettings is not null
                ? new KernelArguments(execSettings)
                : _agent.Arguments
        };

        ChatMessageContent? chatMessageContent = null;
        try
        {
            // Log the agent name and first part of instructions.
            _logger.LogInformation("Invoking agent `{AgentName}` with instructions `{Instructions}`.",
                _agent.Name,
                _agent.Instructions?[..80]);

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

            OpenAI.Chat.ChatTokenUsage? usageDetails = usage as OpenAI.Chat.ChatTokenUsage;

            _logger.LogInformation("Agent `{AuthorName}` with model `{ModelId}` used {InputTokenCount} input tokens and {OutputTokenCount} output tokens.",
            chatMessageContent.AuthorName, chatMessageContent.ModelId, usageDetails?.InputTokenCount, usageDetails?.OutputTokenCount);
        }

        return new AgentResponse(chatMessageContent?.Content, usage, chatMessageContent);
    }

    protected async Task<TResult?> InvokeAndDeserializeAsync<TResult>(ChatMessageContent message,
                                        CancellationToken cancellationToken = default)
    {
        return await InvokeAndDeserializeAsync<TResult>(message, null, null, cancellationToken).ConfigureAwait(false);
    }

    protected async Task<TResult?> InvokeAndDeserializeAsync<TResult>(ChatMessageContent message, Func<string, TResult?>? customDeserializer = null,
                                    OpenAIPromptExecutionSettings? execSettings = null,
                                    CancellationToken cancellationToken = default)
    {
        var resp = await InvokeAgentAsync(message, execSettings, cancellationToken).ConfigureAwait(false);
        OnRawResponse(resp.RawContent, resp.Usage);

        var raw = resp.RawContent;
        if (string.IsNullOrWhiteSpace(raw))
        {
            _logger.LogWarning("Agent returned empty response.");
            return default;
        }

        _logger.LogDebug("Raw agent response: {Raw}", raw);

        // strip code fences if present ...
        if (raw.StartsWith("```"))
        {
            var s = raw.IndexOf('\n'); var e = raw.LastIndexOf("```", StringComparison.Ordinal);
            if (s > 0 && e > s) raw = raw[(s + 1)..e].Trim();
        }

        _logger.LogDebug("Cleaned agent response: {Raw}", raw);

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
