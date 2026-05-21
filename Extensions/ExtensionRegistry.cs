using System.Text.RegularExpressions;

namespace Flow.Extensions;

/// <summary>
/// Static registry that maps trigger patterns to built-in extensions.
/// Used by the main search engine to detect which extension to activate for a given query.
/// </summary>
public static class ExtensionRegistry
{
    private static readonly List<IExtension> Extensions = [];

    static ExtensionRegistry()
    {
        Register(new CalculatorExtension());
        Register(new ClipboardExtension());
        Register(new ColorPickerExtension());
        Register(new TimerExtension());
        Register(new IpAddressExtension());
        Register(new AiAssistantExtension());
        Register(new SettingsExtension());
    }

    /// <summary>
    /// Registers an extension so it can be detected by queries.
    /// </summary>
    public static void Register(IExtension extension)
    {
        Extensions.Add(extension);
    }

    /// <summary>
    /// Returns all registered extensions.
    /// </summary>
    public static IReadOnlyList<IExtension> GetAll() => Extensions.AsReadOnly();

    /// <summary>
    /// Detects which extension (if any) is triggered by the given query.
    /// Returns null if no extension matches.
    /// </summary>
    /// <param name="query">The user's raw search query.</param>
    /// <returns>The matching <see cref="IExtension"/>, or null.</returns>
    public static IExtension? DetectExtension(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        var trimmed = query.Trim();

        foreach (var extension in Extensions)
        {
            var trigger = extension.Trigger;

            if (trigger is string exactPrefix)
            {
                // Case-insensitive prefix match
                if (trimmed.StartsWith(exactPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // For prefix-style triggers, require either an exact match or a space after the prefix
                    // e.g. "clip" matches "clip" and "clip " but not "clipboard"
                    if (trimmed.Length == exactPrefix.Length || trimmed[exactPrefix.Length] == ' ')
                    {
                        return extension;
                    }
                }
            }
            else if (trigger is Regex pattern)
            {
                if (pattern.IsMatch(trimmed))
                {
                    return extension;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds an extension by its unique Id.
    /// </summary>
    public static IExtension? FindById(string id)
    {
        return Extensions.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
