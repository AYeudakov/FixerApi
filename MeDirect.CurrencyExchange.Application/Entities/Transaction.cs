#pragma warning disable CS8618
namespace MeDirect.CurrencyExchange.Application.Entities;

public class Transaction
{
    public int Id { get; set; }
    public DateTime TransactionTime { get; set; }
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public Currency From  { get; set; }
    public Currency To  { get; set; }

    public decimal ExchangeRate { get; set; }
    public int UserId { get; set; }

    public User User { get; set; }
}

public enum Currency
{
    USD,
    EUR,
    GBP,
    PLN
}