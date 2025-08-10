using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentFunction.ClaimsAgent.Controllers;

[ApiController]
[Route("agent/completions")]
public class AgentCompletionsController(ChatCompletionAgent agent, ILogger<AgentCompletionsController> logger) : ControllerBase
{
    private readonly ChatCompletionAgent _agent = agent;

    [HttpPost]
    public async Task<IActionResult> CompleteAsync([FromBody] AgentCompletionRequest request, CancellationToken cancellationToken)
    {
        // Validate input: prompt must not be null or empty for a meaningful completion
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            logger.LogWarning("Received invalid completion request: prompt is missing.");
            return BadRequest("Prompt must be provided.");
        }

        try
        {
            var prompt = request.Prompt;

            // Invoke the agent with the provided prompt.
            IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> content =
                _agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, prompt), cancellationToken: cancellationToken);

            // Process the agent's response. Only concerned with the first item.
            ChatMessageContent? chatMessageContent = null;
            await foreach (AgentResponseItem<ChatMessageContent> item in content.ConfigureAwait(false))
            {
                chatMessageContent = item.Message;
                break;
            }

            var usage = chatMessageContent?.Metadata?["Usage"] as OpenAI.Chat.ChatTokenUsage;
            ShowUsageDetails(usage);

            logger.LogInformation("Agent Response: {Response}", chatMessageContent?.Content);

            return Ok(chatMessageContent);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Completion request was canceled.");
            return StatusCode(StatusCodes.Status499ClientClosedRequest, "Request was canceled.");
        }
        catch (Exception ex)
        {
            // Log the exception for diagnostics, but do not leak details to the client
            logger.LogError(ex, "Error occurred while processing agent completion.");
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
            logger.LogInformation("Input tokens: {InputTokenCount}, Output tokens: {OutputTokenCount}, Total tokens: {TotalTokenCount}",
                usage.InputTokenCount,
                usage.OutputTokenCount,
                usage.TotalTokenCount);
        }
    }
}