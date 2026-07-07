using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SJP.Schematic.Tool.Handlers;

// Resolves `${NAME}`-style placeholders against the process environment. Kept deliberately
// simple (no nesting, no default-value syntax) to avoid building a templating engine.
public static partial class EnvironmentVariableSubstitution
{
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Resolve(string? value)
    {
        if (value == null || !value.Contains("${", StringComparison.Ordinal))
            return value;

        return PlaceholderPattern().Replace(value, match =>
        {
            var name = match.Groups[1].Value;
            var envValue = Environment.GetEnvironmentVariable(name);
            if (envValue == null)
                throw new InvalidOperationException($"Configuration references environment variable '{name}', but it is not set.");

            return envValue;
        });
    }

    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex PlaceholderPattern();
}
