namespace MiniBox.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Test]
    public void Constructor_StoresAmountAndCurrency()
    {
        var money = new MiniBox.Domain.ValueObjects.Money(19.95m, MiniBox.Domain.ValueObjects.Currency.UAH);

        Assert.That(money.Amount, Is.EqualTo(19.95m));
        Assert.That(money.Currency, Is.EqualTo(MiniBox.Domain.ValueObjects.Currency.UAH));
    }

    [Test]
    public void Constructor_RejectsNegativeAmount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MiniBox.Domain.ValueObjects.Money(-0.01m, MiniBox.Domain.ValueObjects.Currency.USD));
    }

    [Test]
    public void Constructor_RejectsUndefinedCurrency()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MiniBox.Domain.ValueObjects.Money(1m, (MiniBox.Domain.ValueObjects.Currency)999));
    }
}
