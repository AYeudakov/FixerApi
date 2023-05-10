using System.Runtime.Serialization;
using Flurl;
using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CurrencyExchange.Application.Services;

public class FixerApiRequester : IFixerApiRequester
{
	private readonly HttpClient _client;
	private readonly ILogger<FixerApiRequester> _logger;

    public FixerApiRequester(IHttpClientFactory httpClientFactory, ILogger<FixerApiRequester> logger)
    {
        _client = httpClientFactory.CreateClient("fixerApi");
        _logger = logger;
    }
    
    public async Task<TransactionInfo> GetConvertResponse(TransactionCreationRequest request)
    {
		var uri = _client.BaseAddress.AppendPathSegment("convert")
				.SetQueryParam(nameof(request.From), request.From)
				.SetQueryParam(nameof(request.To), request.To)
				.SetQueryParam(nameof(request.Amount), request.Amount);

		var transactionInfo = await GetDataFromApiAsync(uri);

        return transactionInfo;
	}

	// Can be change to generic in case of other queries,
	// u just need to create BaseResponse class
	// as domain class for TransactionInfo with property Success and make where T: BaseResponse
    // with property Success
	private async Task<TransactionInfo> GetDataFromApiAsync(string uri)
    {
		TransactionInfo baseTransactionInfo;
        
        try
        {
            var response = await _client.GetStringAsync(uri);
            baseTransactionInfo = JsonConvert.DeserializeObject<TransactionInfo>(response)!;

            if (baseTransactionInfo is not {Success: true})
            {
                throw new SerializationException();
            }
        }
        catch (SerializationException e)
        {
            _logger.LogError(e.Message);
            throw;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e.Message);
            throw;
        }
        
        return baseTransactionInfo; 
    }
}