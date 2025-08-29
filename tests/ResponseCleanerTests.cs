using AgentFunction.Functions.Agents;
using Xunit;

namespace AgentFunction.Tests;

public class ResponseCleanerTests
{
    [Fact(DisplayName = "StripCodeFence_FencedJson_ReturnsInnerJson")]
    public void StripCodeFence_FencedJson_ReturnsInnerJson()
    {
        string input = "```json\n{\"foo\": \"bar\"}\n```";

        var result = ResponseCleaner.StripCodeFence(input);

        Assert.Equal("{\"foo\": \"bar\"}", result);
    }

    [Fact(DisplayName = "StripCodeFence_NoFence_ReturnsOriginal")]
    public void StripCodeFence_NoFence_ReturnsOriginal()
    {
        string input = "just some text";

        var result = ResponseCleaner.StripCodeFence(input);

        Assert.Equal(input, result);
    }

    [Fact(DisplayName = "StripCodeFence_WhitespaceAroundFences_ReturnsInnerTrimmed")]
    public void StripCodeFence_WhitespaceAroundFences_ReturnsInnerTrimmed()
    {
        string input = "   ```\n  abc  \n```   ";

        var result = ResponseCleaner.StripCodeFence(input);

        Assert.Equal("abc", result);
    }

    [Fact(DisplayName = "StripCodeFence_NullOrEmpty_ReturnsSame")]
    public void StripCodeFence_NullOrEmpty_ReturnsSame()
    {
        string? nullInput = null;
        string empty = string.Empty;

        Assert.Null(ResponseCleaner.StripCodeFence(nullInput));
        Assert.Equal(string.Empty, ResponseCleaner.StripCodeFence(empty));
    }
}
