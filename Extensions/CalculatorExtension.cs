using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Flow.Extensions;

/// <summary>
/// Evaluates math expressions and displays the result in large mono font.
/// Uses <see cref="DataTable.Compute"/> for safe expression evaluation.
/// </summary>
public partial class CalculatorExtension : UserControl, IExtension
{
    public string Id => "calculator";
    public string DisplayName => "Calculator";

    /// <summary>
    /// Matches math expressions: digits, operators, parentheses, decimals, and whitespace.
    /// Requires at least one digit and one operator to prevent single-number matches.
    /// </summary>
    public object Trigger => new Regex(
        @"^[\d\s\+\-\*\/\%\(\)\.\,\^]+$",
        RegexOptions.Compiled);

    public CalculatorExtension()
    {
        InitializeComponent();
    }

    public UserControl CreateControl(string query)
    {
        var control = new CalculatorExtension();
        control.Evaluate(query.Trim());
        return control;
    }

    /// <summary>
    /// Evaluates a math expression safely using DataTable.Compute.
    /// Falls back gracefully on parse errors.
    /// </summary>
    private void Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            ShowResult("0");
            return;
        }

        ExpressionText.Text = expression;

        try
        {
            // Sanitize the expression: only allow numeric characters and safe operators
            var sanitized = SanitizeExpression(expression);

            if (string.IsNullOrEmpty(sanitized))
            {
                ShowError("Invalid expression");
                return;
            }

            // Use DataTable.Compute as a safe expression evaluator
            var dataTable = new DataTable();
            var result = dataTable.Compute(sanitized, null);

            if (result is double d)
            {
                // Format: remove unnecessary decimal places for whole numbers
                ShowResult(d == Math.Floor(d) && !double.IsInfinity(d)
                    ? d.ToString("N0")
                    : d.ToString("G15"));
            }
            else
            {
                ShowResult(result?.ToString() ?? "0");
            }
        }
        catch (Exception)
        {
            ShowError("Invalid expression");
        }
    }

    /// <summary>
    /// Removes any characters that are not part of a safe math expression.
    /// </summary>
    private static string SanitizeExpression(string input)
    {
        // Remove all characters that are not: digits, operators, parens, decimal, whitespace, caret
        var safe = Regex.Replace(input, @"[^\d\s\+\-\*\/\%\(\)\.\,\^]", "");

        if (string.IsNullOrWhiteSpace(safe))
            return string.Empty;

        // Replace ^ with nothing (DataTable.Compute doesn't support ^)
        // Replace commas with nothing
        safe = safe.Replace("^", "").Replace(",", "");

        return safe.Trim();
    }

    private void ShowResult(string result)
    {
        ResultText.Text = $"= {result}";
        ErrorText.Visibility = System.Windows.Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ResultText.Text = "";
        ErrorText.Text = message;
        ErrorText.Visibility = System.Windows.Visibility.Visible;
    }
}
