using Flow.Models;

namespace Flow.Services;

/// <summary>
/// A fast, simple fuzzy-matching engine for scoring and ranking search results.
/// Searches for all query characters in order within the target string and returns
/// a relevance score that rewards consecutive matches and word-start matches.
/// </summary>
public static class FuzzySearch
{
    /// <summary>
    /// The weight multiplier applied to the match-position score (consecutive bonus / gap penalty).
    /// </summary>
    private const double PositionWeight = 0.6;

    /// <summary>
    /// The weight multiplier applied to the word-start bonus.
    /// </summary>
    private const double WordStartWeight = 0.4;

    /// <summary>
    /// Bonus points awarded when a matched character is at the start of a word.
    /// This value is scaled by <see cref="WordStartWeight"/>.
    /// </summary>
    private const double WordStartBonus = 1.0;

    /// <summary>
    /// Bonus points awarded when two matched characters are consecutive.
    /// This value is scaled by <see cref="PositionWeight"/>.
    /// </summary>
    private const double ConsecutiveBonus = 0.5;

    /// <summary>
    /// Penalty applied per gap (number of skipped characters between matches).
    /// This value is scaled by <see cref="PositionWeight"/>.
    /// </summary>
    private const double GapPenalty = 0.1;

    /// <summary>
    /// Performs a fuzzy search across a collection of items and returns them
    /// sorted by score (descending), then by frequency score (descending).
    /// </summary>
    /// <typeparam name="T">The item type. Must expose a Name property via <paramref name="nameSelector"/>.</typeparam>
    /// <param name="items">The collection of items to search.</param>
    /// <param name="query">The user's search query.</param>
    /// <param name="nameSelector">A function that extracts the searchable name from each item.</param>
    /// <param name="frequencySelector">
    /// An optional function that provides a frequency/recency score for each item.
    /// Higher values rank higher when fuzzy scores tie.
    /// </param>
    /// <returns>A list of items whose names fuzzy-match the query, sorted by relevance.</returns>
    public static List<T> Search<T>(
        IEnumerable<T> items,
        string query,
        Func<T, string> nameSelector,
        Func<T, double>? frequencySelector = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return items.ToList();

        var results = new List<(T Item, double Score)>();

        foreach (var item in items)
        {
            var name = nameSelector(item);
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var score = Score(query, name);
            if (score > 0)
            {
                // Blend in frequency score if available
                if (frequencySelector is not null)
                {
                    var freq = frequencySelector(item);
                    score += freq * 0.01; // Small influence — tie-breaker
                }

                results.Add((item, score));
            }
        }

        return results
            .OrderByDescending(r => r.Score)
            .Select(r => r.Item)
            .ToList();
    }

    /// <summary>
    /// Computes a fuzzy-match score between a query string and a target string.
    /// All characters in <paramref name="query"/> must appear in order in
    /// <paramref name="target"/> for any score above zero.
    /// </summary>
    /// <param name="query">The user's query (typically lowercase).</param>
    /// <param name="target">The target string to match against.</param>
    /// <returns>
    /// A score >= 0. Higher is better. 0 means no match.
    /// </returns>
    public static double Score(string query, string target)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(target))
            return 0;

        var querySpan = query.AsSpan().Trim();
        var targetSpan = target.AsSpan().Trim();

        if (querySpan.IsEmpty || targetSpan.IsEmpty)
            return 0;

        var queryIndex = 0;
        var lastMatchIndex = -1;
        double positionScore = 0;
        double wordStartScore = 0;
        bool matched = false;

        for (var targetIndex = 0; targetIndex < targetSpan.Length; targetIndex++)
        {
            if (queryIndex >= querySpan.Length)
                break;

            if (char.ToLowerInvariant(targetSpan[targetIndex]) == char.ToLowerInvariant(querySpan[queryIndex]))
            {
                matched = true;

                // ---- Position score ----
                if (lastMatchIndex >= 0)
                {
                    var gap = targetIndex - lastMatchIndex;
                    if (gap == 1)
                    {
                        // Consecutive matches
                        positionScore += ConsecutiveBonus;
                    }
                    else
                    {
                        // Penalty for gaps
                        positionScore -= gap * GapPenalty;
                    }
                }

                // ---- Word-start bonus ----
                if (targetIndex == 0 || IsWordBoundary(targetSpan[targetIndex - 1]))
                {
                    wordStartScore += WordStartBonus;
                }

                lastMatchIndex = targetIndex;
                queryIndex++;
            }
        }

        // Require all query characters to be matched
        if (queryIndex < querySpan.Length)
            return 0;

        if (!matched)
            return 0;

        // Normalise: scale by query length so short queries aren't penalised
        var normalisedPosition = positionScore / querySpan.Length;
        var normalisedWordStart = wordStartScore / querySpan.Length;

        var finalScore = (normalisedPosition * PositionWeight) + (normalisedWordStart * WordStartWeight);

        // Small bonus for matching the exact query length (favours tighter matches)
        var lengthRatio = (double)querySpan.Length / targetSpan.Length;
        finalScore += lengthRatio * 0.1;

        return Math.Max(0, Math.Round(finalScore, 4));
    }

    /// <summary>
    /// Determines whether a character is a word boundary (space, underscore, hyphen, or dot).
    /// </summary>
    private static bool IsWordBoundary(char c)
    {
        return c == ' ' || c == '_' || c == '-' || c == '.';
    }

    // ---- Convenience overloads for SearchResult ----

    /// <summary>
    /// Fuzzy-searches a list of <see cref="SearchResult"/> items and returns them ranked.
    /// </summary>
    /// <param name="items">The search results to rank.</param>
    /// <param name="query">The user's search query.</param>
    /// <returns>Filtered and ranked results.</returns>
    public static List<SearchResult> RankResults(List<SearchResult> items, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            // No query: return sorted by frequency score descending
            return items.OrderByDescending(r => r.FrequencyScore).ToList();
        }

        var scored = new List<(SearchResult Item, double Score)>();

        foreach (var item in items)
        {
            var score = Score(query, item.Name);
            if (score > 0)
            {
                score += item.FrequencyScore * 0.01;
                scored.Add((item, score));
            }
        }

        return scored
            .OrderByDescending(r => r.Score)
            .Select(r =>
            {
                r.Item.Score = r.Score;
                return r.Item;
            })
            .ToList();
    }
}
