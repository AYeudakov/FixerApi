namespace MeDirect.CurrencyExchange.Domain.Models;

public class TransactionInfo
{
    public DateTime Date { get; set; }
    public decimal Result { get; set; }
    public Query Query { get; set; } = new();
    public Info Info { get; set; } = new();
    public bool Success { get; set; }
}

public class Query
{
    public decimal Amount { get; set; }
    public DomainCurrency From { get; set; }
    public DomainCurrency To { get; set; }
}

public class Info
{
    public decimal Rate { get; set; }
    public long Timestamp { get; set; }
}