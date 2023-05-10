using CurrencyExchange.Application.Entities;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Interfaces;

public interface IExchangeCurrencyService
{
	public Task<TransactionInfo> ExchangeCurrency(TransactionCreationRequest request, int userId);
}