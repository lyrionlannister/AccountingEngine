using Accounting.Domain;

namespace Accounting.Tests;

public class ExchangeRateTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var rate = ExchangeRate.Create(1, 2, 18.5m, new DateOnly(2026, 10, 23), "Banxico");

        Assert.Equal(1, rate.FromCurrencyId);
        Assert.Equal(2, rate.ToCurrencyId);
        Assert.Equal(18.5m, rate.Rate);
        Assert.Equal("Banxico", rate.Source);
    }

    [Fact]
    public void Create_WithSameFromAndToCurrency_Throws()
    {
        Assert.Throws<ArgumentException>(() => ExchangeRate.Create(1, 1, 1m, new DateOnly(2026, 10, 23), "Banxico"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveRate_Throws(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExchangeRate.Create(1, 2, rate, new DateOnly(2026, 10, 23), "Banxico"));
    }

    [Fact]
    public void Create_WithoutSource_Throws()
    {
        Assert.Throws<ArgumentException>(() => ExchangeRate.Create(1, 2, 1m, new DateOnly(2026, 10, 23), ""));
    }
}
