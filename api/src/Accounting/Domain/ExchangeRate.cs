namespace Accounting.Domain;

public sealed class ExchangeRate
{
    public long Id { get; private set; }
    public long FromCurrencyId { get; private set; }
    public long ToCurrencyId { get; private set; }
    public decimal Rate { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public string Source { get; private set; } = string.Empty;

    private ExchangeRate()
    {
    }

    public static ExchangeRate Create(long fromCurrencyId, long toCurrencyId, decimal rate, DateOnly effectiveDate, string source)
    {
        if (fromCurrencyId == toCurrencyId)
            throw new ArgumentException("A currency cannot have an exchange rate against itself.");
        if (rate <= 0)
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be greater than zero.");
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source is required.", nameof(source));

        return new ExchangeRate
        {
            FromCurrencyId = fromCurrencyId,
            ToCurrencyId = toCurrencyId,
            Rate = rate,
            EffectiveDate = effectiveDate,
            Source = source
        };
    }
}
