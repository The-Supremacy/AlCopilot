using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Domain.ValueObjects;

public sealed class DrinkNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsValueObject()
    {
        var name = DrinkName.Create("Margarita");
        name.Value.ShouldBe("Margarita");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var name = DrinkName.Create("  Margarita  ");
        name.Value.ShouldBe("Margarita");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrNull_Throws(string? value)
    {
        Should.Throw<ArgumentException>(() => DrinkName.Create(value!));
    }

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        var longName = new string('a', 201);
        Should.Throw<ArgumentException>(() => DrinkName.Create(longName));
    }

    [Fact]
    public void ImplicitConversion_ReturnsStringValue()
    {
        var name = DrinkName.Create("Mojito");
        string result = name;
        result.ShouldBe("Mojito");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = DrinkName.Create("Mojito");
        var b = DrinkName.Create("Mojito");
        a.ShouldBe(b);
    }
}

public sealed class TagNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsValueObject()
    {
        TagName.Create("Classic").Value.ShouldBe("Classic");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithEmpty_Throws(string? value)
    {
        Should.Throw<ArgumentException>(() => TagName.Create(value!));
    }

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        Should.Throw<ArgumentException>(() => TagName.Create(new string('a', 101)));
    }
}

public sealed class IngredientNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsValueObject()
    {
        IngredientName.Create("Lime Juice").Value.ShouldBe("Lime Juice");
    }

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        Should.Throw<ArgumentException>(() => IngredientName.Create(new string('a', 201)));
    }
}

public sealed class CategoryNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsValueObject()
    {
        CategoryName.Create("Spirits").Value.ShouldBe("Spirits");
    }

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        Should.Throw<ArgumentException>(() => CategoryName.Create(new string('a', 101)));
    }
}

public sealed class QuantityTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsValueObject()
    {
        Quantity.Create("2 oz").Value.ShouldBe("2 oz");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmpty_Throws(string? value)
    {
        Should.Throw<ArgumentException>(() => Quantity.Create(value!));
    }

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        Should.Throw<ArgumentException>(() => Quantity.Create(new string('a', 101)));
    }
}

public sealed class ImageUrlTests
{
    [Fact]
    public void Create_WithValidUrl_ReturnsValueObject()
    {
        ImageUrl.Create("https://example.com/img.jpg").Value.ShouldBe("https://example.com/img.jpg");
    }

    [Fact]
    public void Create_WithNull_ReturnsNullValue()
    {
        ImageUrl.Create(null).Value.ShouldBeNull();
    }

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        Should.Throw<ArgumentException>(() => ImageUrl.Create(new string('a', 1001)));
    }
}
