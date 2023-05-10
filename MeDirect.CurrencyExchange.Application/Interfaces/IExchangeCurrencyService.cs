using MeDirect.CurrencyExchange.Application.Entities;
using MeDirect.CurrencyExchange.Domain.Models;

namespace MeDirect.CurrencyExchange.Application.Interfaces;

public interface IExchangeCurrencyService
{
    public Task<TransactionInfo> ExchangeCurrency(TransactionCreationRequest request, int userId);
}