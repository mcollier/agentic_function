using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.ClaimsAgent.Controllers;

[ApiController]
[Route("agent/completions")]
public class AgentCompletionsController(ChatCompletionAgent agent, ILogger<AgentCompletionsController> logger) : ControllerBase
{
    private readonly ChatCompletionAgent _agent = agent;
    private readonly ILogger<AgentCompletionsController> _logger = logger;

    [HttpPost]
    public async Task<IActionResult> CompleteAsync([FromBody] AgentCompletionRequest request, CancellationToken cancellationToken)
    {
        // Validate input: prompt must not be null or empty for a meaningful completion
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            _logger.LogWarning("Received invalid completion request: prompt is missing.");
            return BadRequest("Prompt must be provided.");
        }

        try
        {
            var prompt = request.Prompt;

            AgentResponseItem<ChatMessageContent>? responseItem = null;
            IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> content =
                _agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, prompt), cancellationToken: cancellationToken);

            ChatMessageContent? chatMessageContent = null;
            await foreach (ChatMessageContent item in content.ConfigureAwait(false))
            {
                chatMessageContent = item;
                break;
            }

            // var usage = responseItem?.Message?.Metadata?["Usage"] as UsageDetails;
            var usage = chatMessageContent?.Metadata?["Usage"] as OpenAI.Chat.ChatTokenUsage;
            

            _logger.LogInformation("Agent Response: {Response}", responseItem?.Message?.Content);

            return Ok(chatMessageContent);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Completion request was canceled.");
            return StatusCode(StatusCodes.Status499ClientClosedRequest, "Request was canceled.");
        }
        catch (Exception ex)
        {
            // Log the exception for diagnostics, but do not leak details to the client
            _logger.LogError(ex, "Error occurred while processing agent completion.");
            return Problem(
                detail: "An unexpected error occurred while processing your request.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Agent Completion Error"
            );
        }
    }

    /// <summary>
    /// Logs usage details if available.
    /// </summary>
    /// <param name="usage">The usage details object.</param>
    private void ShowUsageDetails(OpenAI.Chat.ChatTokenUsage? usage)
    {
        if (usage is not null)
        {
            _logger.LogInformation("Input tokens: {InputTokenCount}, Output tokens: {OutputTokenCount}, Total tokens: {TotalTokenCount}",
                usage.InputTokenCount,
                usage.OutputTokenCount,
                usage.TotalTokenCount);

            // if (usage.AdditionalCounts is not null && usage.AdditionalCounts.Count > 0)
            // {
            //     foreach (var kvp in usage.AdditionalCounts)
            //     {
            //         _logger.LogInformation("Additional count - {Key}: {Value}", kvp.Key, kvp.Value);
            //     }
            // }
        }
    }
}