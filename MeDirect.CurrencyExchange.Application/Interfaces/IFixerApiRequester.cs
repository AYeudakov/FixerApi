using MeDirect.CurrencyExchange.Application.Entities;
using MeDirect.CurrencyExchange.Domain.Models;

namespace MeDirect.CurrencyExchange.Application.Interfaces
{
	public interface IFixerApiRequester
	{
		public Task<TransactionInfo> GetConvertResponse(TransactionCreationRequest request);
	}
}
