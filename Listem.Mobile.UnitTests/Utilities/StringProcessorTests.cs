using Listem.Mobile.Utilities;

namespace Listem.Mobile.UnitTests.Utilities;

[TestFixture]
public class StringProcessorTests
{
  [Test]
  public void TrimAndCapitalise_TrimsAndCapitalisesFirstLetter()
  {
    var processed = StringProcessor.TrimAndCapitalise("  apples  ");

    Assert.That(processed, Is.EqualTo("Apples"));
  }

  [Test]
  public void TrimAndCapitalise_SingleCharacter_UppercasesCharacter()
  {
    var processed = StringProcessor.TrimAndCapitalise(" z ");

    Assert.That(processed, Is.EqualTo("Z"));
  }

  [Test]
  public void ExtractItem_WithQuantityAndImportant_ExtractsAllParts()
  {
    var (name, quantity, isImportant) = StringProcessor.ExtractItem("Bread (3)!");

    Assert.Multiple(() =>
    {
      Assert.That(name, Is.EqualTo("Bread"));
      Assert.That(quantity, Is.EqualTo(3));
      Assert.That(isImportant, Is.True);
    });
  }

  [Test]
  public void ExtractItem_WithoutMarkers_UsesDefaults()
  {
    var (name, quantity, isImportant) = StringProcessor.ExtractItem("Milk");

    Assert.Multiple(() =>
    {
      Assert.That(name, Is.EqualTo("Milk"));
      Assert.That(quantity, Is.EqualTo(1));
      Assert.That(isImportant, Is.False);
    });
  }

  [Test]
  public void IsCategoryName_BracketedCategory_ReturnsTrue()
  {
    Assert.That(StringProcessor.IsCategoryName("[Dairy]:"), Is.True);
  }

  [Test]
  public void ExtractCategoryName_InvalidFormat_ReturnsDefaultCategory()
  {
    var category = StringProcessor.ExtractCategoryName("Not a category");

    Assert.That(category, Is.EqualTo(Constants.DefaultCategoryName));
  }
}
