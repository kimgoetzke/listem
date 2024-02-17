using System.Text.RegularExpressions;
using Listem.Services;

namespace Listem.Utilities;

public static partial class StringProcessor
{
    [GeneratedRegex(@"\[(.*)\]:")]
    private static partial Regex StoreRegex();

    [GeneratedRegex(@"^(.*?)(?=\s*\(\d+|\s*!|$)")]
    private static partial Regex ItemNameRegex();

    [GeneratedRegex(@"(\d+)")]
    private static partial Regex ItemQuantityRegex();

    [GeneratedRegex(@"!")]
    private static partial Regex ItemIsImportantRegex();

    public static string TrimAndCapitaliseFirstChar(string s)
    {
        var trimmed = s.Trim();
        return s.Length > 1 ? trimmed[..1].ToUpper() + trimmed[1..] : trimmed.ToUpper();
    }

    public static (string, int, bool) ExtractItem(string input)
    {
        var itemNameMatch = ItemNameRegex().Match(input);
        var itemName = itemNameMatch.Success
            ? itemNameMatch.Groups[1].Value.Trim()
            : "<Failed to extract>";
        var quantityMatch = ItemQuantityRegex().Match(input);
        var quantity = ParseMatchToIntOr1(quantityMatch);
        var isImportant = ItemIsImportantRegex().IsMatch(input);
        return (itemName, quantity, isImportant);
    }

    private static int ParseMatchToIntOr1(Capture match)
    {
        var success = int.TryParse(match.Value, out var number);
        return success ? number : 1;
    }

    public static bool IsStoreName(string input)
    {
        var match = StoreRegex().Match(input);
        return match.Success;
    }

    public static string ExtractStoreName(string input)
    {
        var match = StoreRegex().Match(input);
        return match.Success ? match.Groups[1].Value.Trim() : IStoreService.DefaultStoreName;
    }
}
