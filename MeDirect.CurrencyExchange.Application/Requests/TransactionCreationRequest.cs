namespace MeDirect.CurrencyExchange.Application.Entities;

public class TransactionCreationRequest
{
    public decimal Amount { get; set; }
    public Currency From  { get; set; }
    public Currency To  { get; set; }
    
}