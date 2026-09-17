using System.Text.RegularExpressions;

namespace ERP_Government.Web.Infrastructure;

/// <summary>
/// Converts backend PascalCase bracket-notation field paths to canonical frontend camelCase dot-notation.
/// Examples: "Lines[0].Amount" → "lines.0.amount", "Name" → "name"
/// </summary>
public static partial class FieldPathCanonicalizer
{
    [GeneratedRegex(@"\[(\d+)\]")]
    private static partial Regex ArrayIndexRegex();

    /// <summary>
    /// Canonicalize a backend field path to the frontend canonical format.
    /// </summary>
    public static string Canonicalize(string backendPath)
    {
        if (string.IsNullOrWhiteSpace(backendPath))
            return backendPath;

        var result = ArrayIndexRegex().Replace(backendPath, ".$1");
        result = ToCamelCase(result);
        return result;
    }

    /// <summary>
    /// Canonicalize a dictionary of field paths (e.g., from ValidationException.Errors).
    /// </summary>
    public static Dictionary<string, string[]> CanonicalizeKeys(IDictionary<string, string[]> errors)
    {
        var canonicalized = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var (key, messages) in errors)
        {
            var canonicalKey = string.IsNullOrEmpty(key) ? key : Canonicalize(key);
            if (!canonicalized.ContainsKey(canonicalKey))
            {
                canonicalized[canonicalKey] = messages;
            }
            else
            {
                canonicalized[canonicalKey] = canonicalized[canonicalKey]
                    .Concat(messages)
                    .Distinct()
                    .ToArray();
            }
        }

        return canonicalized;
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var segments = input.Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = ToCamelCaseSegment(segments[i]);
        }

        return string.Join('.', segments);
    }

    private static string ToCamelCaseSegment(string segment)
    {
        if (string.IsNullOrEmpty(segment))
            return segment;

        if (segment.Length == 1)
            return segment.ToLowerInvariant();

        if (char.IsUpper(segment[0]) && char.IsUpper(segment[1]))
            return segment;

        return char.ToLowerInvariant(segment[0]) + segment[1..];
    }
}
