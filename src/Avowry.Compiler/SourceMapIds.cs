using System.Security.Cryptography;
using System.Text;

namespace Avowry.Compiler;

/// <summary>Shared step id recipe for the CLI source map and generated failure observers.</summary>
public static class SourceMapIds
{
    public static string StepId(string kind, int line, string name, string endpoint)
    {
        var normalizedName = name.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var input = $"{kind}\n{line}\n{normalizedName}" + (kind == "failure" ? $"\n{endpoint}" : "");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)))[..12].ToLowerInvariant();
    }
}