using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Flow.Services;

/// <summary>
/// Calls the Groq API (llama3-8b-8192) and streams the response token-by-token via a callback.
/// Reads the API key from the FLOW_GROQ_API_KEY environment variable.
/// </summary>
public static class AiService
{
    private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "llama3-8b-8192";
    private const int MaxQueryLength = 2000;

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Sends a query to the Groq API and streams the response.
    /// </summary>
    /// <param name="query">The user's prompt (max 2000 characters; longer input is truncated).</param>
    /// <param name="onToken">Callback invoked for each token chunk as it arrives from the stream.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>
    /// True if the call completed successfully (even if no tokens were produced);
    /// false if there was an error (no internet, bad key, etc.).
    /// </returns>
    public static async Task<bool> QueryAsync(
        string query,
        Action<string> onToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var apiKey = Environment.GetEnvironmentVariable("FLOW_GROQ_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            onToken("[Error: FLOW_GROQ_API_KEY environment variable is not set.]");
            return false;
        }

        if (query.Length > MaxQueryLength)
            query = query[..MaxQueryLength];

        try
        {
            var requestBody = new
            {
                model = Model,
                messages = new[]
                {
                    new { role = "system", content = "You are Flow, a helpful desktop assistant. Keep answers concise." },
                    new { role = "user", content = query }
                },
                stream = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, GroqApiUrl)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await HttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var friendlyError = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "[Error: Invalid API key. Check your FLOW_GROQ_API_KEY.]",
                    System.Net.HttpStatusCode.TooManyRequests => "[Error: Rate limited. Please wait a moment and try again.]",
                    _ => $"[Error: API returned {(int)response.StatusCode}. {errorBody.Truncate(200)}]"
                };
                onToken(friendlyError);
                return false;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var accumulatedContent = new StringBuilder();

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // SSE format: "data: {...}"
                if (!line.StartsWith("data: ", StringComparison.Ordinal))
                    continue;

                var data = line["data: ".Length..];

                // Stream termination signal
                if (data == "[DONE]")
                    break;

                try
                {
                    using var doc = JsonDocument.Parse(data);
                    var choices = doc.RootElement.GetProperty("choices");

                    if (choices.GetArrayLength() == 0)
                        continue;

                    var delta = choices[0].GetProperty("delta");

                    if (delta.TryGetProperty("content", out var contentElement))
                    {
                        var token = contentElement.GetString();
                        if (!string.IsNullOrEmpty(token))
                        {
                            accumulatedContent.Append(token);
                            onToken(token);
                        }
                    }
                }
                catch (JsonException)
                {
                    // Skip malformed SSE lines
                }
            }

            return accumulatedContent.Length > 0;
        }
        catch (TaskCanceledException)
        {
            onToken("[Cancelled]");
            return false;
        }
        catch (HttpRequestException)
        {
            onToken("[Error: Unable to reach the AI service. Check your internet connection.]");
            return false;
        }
        catch (Exception ex)
        {
            onToken($"[Error: {ex.Message.Truncate(200)}]");
            return false;
        }
    }

    /// <summary>
    /// Truncates a string to the specified maximum length, appending "…" if truncated.
    /// </summary>
    private static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;
        return value[..maxLength] + "…";
    }
}
