using System.Text.RegularExpressions;

namespace AgentFunction.Functions.Agents;

internal static partial class ResponseCleaner
{
    // Matches triple-backtick code fences with optional language and captures inner content.
    [GeneratedRegex(@"^\s*```[a-zA-Z0-9]*\s*\n([\s\S]*?)\n?```\s*$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex CodeFenceRegexFactory();

    private static readonly Regex CodeFenceRegex = CodeFenceRegexFactory();

    /// <summary>
    /// If <paramref name="raw"/> is a single code-fenced block, return the inner content trimmed.
    /// Otherwise return the original value (including when null or empty).
    /// </summary>
    /// <param name="raw">Raw agent response</param>
    /// <returns>Cleaned string or original input.</returns>
    public static string? StripCodeFence(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return raw;
        }

        var match = CodeFenceRegex.Match(raw);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return raw;
    }
}
