using System.Text.Json.Serialization;

namespace CurrencyExchange.Domain.Models;

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

	[JsonConverter(typeof(JsonStringEnumConverter))]

	public DomainCurrency From { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter))]

	public DomainCurrency To { get; set; }
}

public class Info
{
    public decimal Rate { get; set; }
    public long Timestamp { get; set; }
}