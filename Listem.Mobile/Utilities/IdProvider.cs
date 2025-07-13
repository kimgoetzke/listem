namespace Listem.Mobile.Utilities;

public static class IdProvider
{
  private const string Separator = "~";
  private const int Length = 3;
  private static readonly Dictionary<string, string> Abbreviations = [];

  public static string NewId(string className)
  {
    var abbreviation = GetOrGeneratePrefix(className);
    return abbreviation + Separator + Guid.NewGuid().ToString("N");
  }

  private static string GetOrGeneratePrefix(string className)
  {
    if (Abbreviations.TryGetValue(className, out var abbreviation))
      return abbreviation;

    abbreviation = GeneratePrefix(className);
    Abbreviations[className] = abbreviation;
    return abbreviation;
  }

  private static string GeneratePrefix(string className)
  {
    for (var offset = 0; offset < className.Length; offset++)
    {
      var abbreviation = className.Substring(offset, Length).ToUpper();
      if (!Abbreviations.ContainsValue(abbreviation))
      {
        return abbreviation;
      }
    }
    throw new InvalidOperationException($"No unique abbreviation for class {className} possible");
  }
}
