namespace Flow.Extensions;

/// <summary>
/// Defines the contract for a Flow extension that is triggered by a query pattern
/// and renders a UserControl to display its results.
/// </summary>
public interface IExtension
{
    /// <summary>
    /// Unique identifier for this extension (e.g. "calculator", "clipboard").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable display name shown in the UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The trigger that activates this extension.
    /// Can be a plain string prefix (e.g. "clip") or a <see cref="System.Text.RegularExpressions.Regex"/> pattern.
    /// </summary>
    object Trigger { get; }

    /// <summary>
    /// Creates the WPF <see cref="System.Windows.Controls.UserControl"/> that renders the extension's UI.
    /// </summary>
    /// <param name="query">The full user query string (trigger prefix included).</param>
    /// <returns>A UserControl instance ready for display.</returns>
    System.Windows.Controls.UserControl CreateControl(string query);
}
