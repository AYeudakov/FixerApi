using AutoMapper;
using Flurl;
using CurrencyExchange.Application.Common.Interfaces;
using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Application.Services;

public class ExchangeCurrencyService : IExchangeCurrencyService
{
	private readonly IFixerApiRequester _fixerApiRequester;
	private readonly IApplicationDbContext _applicationDbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<ExchangeCurrencyService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ExchangeCurrencyService(
		IFixerApiRequester fixerApiRequester,
		IApplicationDbContext applicationDbContext,
        IMapper mapper,
        ILogger<ExchangeCurrencyService> logger,
        IMemoryCache cache,
        IDateTimeProvider dateTimeProvider)
    {
        _fixerApiRequester = fixerApiRequester;
		_applicationDbContext = applicationDbContext;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TransactionInfo> ExchangeCurrency(TransactionCreationRequest request, int userId)
    {
        ValidateNumberOfRequestPerHour(userId);

        if (!TryGetDataFromCache(request, out var transactionInfo))
        {
            transactionInfo = await _fixerApiRequester.GetConvertResponse(request);

            _cache.Set(string.Concat(request.From, request.To),
                transactionInfo, TimeSpan.FromMinutes(30));
        }

        var transaction = _mapper.Map<Transaction>(transactionInfo);
        transaction.UserId = userId;

        await SaveTransactionInfoAsync(transaction);

        return transactionInfo;
    }
    
    private void ValidateNumberOfRequestPerHour(int userId)
    {
        if (GetNumberOfUserTradesPerHourAsync(userId) <= 10)
        {
            return;
        }
        
        string message = "Number of transactions exceeded 10 trades for last hour";
        _logger.LogWarning(message);

        throw new NumberOfRequestExceededException(message);
    }
    
    private int GetNumberOfUserTradesPerHourAsync(int userId)
    {
        return _applicationDbContext.Transactions.Select(t =>
            t.UserId == userId &&
            t.TransactionTime <= DateTime.UtcNow.AddHours(1) &&
            t.TransactionTime >= DateTime.UtcNow).Count();
    }

    private bool TryGetDataFromCache(TransactionCreationRequest request,
        out TransactionInfo transactionInfo)
    {
        if (!_cache.TryGetValue(string.Concat(request.From, request.To),
                out transactionInfo!))
        {
            return false;
        }

        DateTime currentDateTime = _dateTimeProvider.GetCurrentDateTime(); 
        
        transactionInfo!.Query.Amount = request.Amount;
        transactionInfo.Result = transactionInfo.Info.Rate * request.Amount;
        transactionInfo.Info.Timestamp = currentDateTime.Ticks;
        transactionInfo.Date = currentDateTime;

        return true;
    }

    private async Task SaveTransactionInfoAsync(Transaction transaction)
    {
        try
        {
            _applicationDbContext.Transactions.Add(transaction);
            await _applicationDbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogError("Exception during saving of transaction: {message}", e.Message);
            throw;
        }
    }
}