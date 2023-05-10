using CurrencyExchange.Application.Entities;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Interfaces
{
	public interface IFixerApiRequester
	{
		public Task<TransactionInfo> GetConvertResponse(TransactionCreationRequest request);
	}
}
