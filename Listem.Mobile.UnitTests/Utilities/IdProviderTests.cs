using System.Reflection;
using System.Text.RegularExpressions;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.UnitTests.Utilities;

[TestFixture]
public class IdProviderTests
{
  [SetUp]
  public void ResetIdPrefixes()
  {
    var fieldInfo = typeof(IdProvider).GetField(
      "Abbreviations",
      BindingFlags.NonPublic | BindingFlags.Static
    );
    var abbreviations = fieldInfo?.GetValue(null) as Dictionary<string, string>;
    Assert.That(abbreviations, Is.Not.Null, "Expected IdProvider abbreviations dictionary");
    abbreviations!.Clear();
  }

  [Test]
  public void NewId_UsesUppercaseThreeLetterPrefixAndGuid()
  {
    var identifier = IdProvider.NewId("ShoppingList");

    Assert.That(identifier, Does.Match(new Regex("^[A-Z]{3}~[a-f0-9]{32}$")));
  }

  [Test]
  public void NewId_SameClass_ReusesExistingPrefix()
  {
    var firstIdentifier = IdProvider.NewId("CategoryManager");
    var secondIdentifier = IdProvider.NewId("CategoryManager");

    Assert.That(secondIdentifier.Split("~")[0], Is.EqualTo(firstIdentifier.Split("~")[0]));
  }

  [Test]
  public void NewId_WhenPrefixCollides_GeneratesDifferentPrefix()
  {
    var firstIdentifier = IdProvider.NewId("ClassOne");
    var secondIdentifier = IdProvider.NewId("ClassTwo");

    Assert.That(secondIdentifier.Split("~")[0], Is.Not.EqualTo(firstIdentifier.Split("~")[0]));
  }
}
