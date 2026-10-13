namespace MiniBox.Domain.ValueObjects;

public enum Currency
{
    EUR = 1,
    USD = 2,
    UAH = 3
}

public class Money
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (!Enum.IsDefined(currency))
        {
            throw new ArgumentOutOfRangeException(nameof(currency));
        }
        Amount = amount;
        Currency = currency;
    }

}