using System.ComponentModel;
using System.Text;

using Microsoft.SemanticKernel;

namespace AgentFunction.Functions.Plugins;

public sealed class SchemaTools
{
    // Returns the FNOL JSON Schema as plain text (for the agent to reason over)
    [KernelFunction("get_fnol_schema")]
    [Description("Get the FNOL JSON schema as plain text that is to be used for claim validation.")]
    public static Task<string> GetFnolSchemaAsync()
    {
        // TODO: Move to Azure Storage blob?

        var path = "fnol.schema.json";
        return File.ReadAllTextAsync(path, Encoding.UTF8);
    }

}
