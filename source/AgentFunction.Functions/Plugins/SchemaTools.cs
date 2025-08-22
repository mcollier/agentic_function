using System.ComponentModel;
using System.Text;

using Microsoft.SemanticKernel;

namespace AgentFunction.Functions.Plugins;

public sealed class SchemaTools
{
    // Returns the FNOL JSON Schema as plain text (for the agent to reason over)
    [KernelFunction, Description("Get the FNOL JSON schema used for validation.")]
    public static Task<string> GetFnolSchemaAsync()
    {
        // TODO: Move to Azure Storage blob?

        // var path = Environment.GetEnvironmentVariable("FNOL_SCHEMA_PATH")
        //           ?? throw new InvalidOperationException("FNOL_SCHEMA_PATH not set");
        var path = "fnol.schema.json";
        return Task.FromResult(File.ReadAllText(path, Encoding.UTF8));
    }

    // Returns canonical enum values for specific fields (extend as needed)
    [KernelFunction, Description("List canonical enum values for fields (e.g., party roles).")]
    public static Task<string[]> GetEnumValuesAsync(
        [Description("Field name (e.g., 'party.role')")] string field)
    {
        return Task.FromResult(field.ToLowerInvariant() switch
        {
            "party.role" => ["insured", "third_party", "witness", "claimant"],
            _ => Array.Empty<string>()
        });
    }
}
