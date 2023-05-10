using AutoFixture;
using AutoFixture.NUnit3;
using AutoMapper;
using CurrencyExchange.Application.Common.Interfaces;
using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Services;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CurrencyExchange.Application.UnitTests.Services;

public class ExchangeCurrencyServiceTest
{
    private Mock<IMapper> _mapperMock;
    private Mock<IApplicationDbContext> _appDbContextMock;
    private Mock<IFixerApiRequester> _fixerApiServiceMock;
    private Mock<IDateTimeProvider> _dateTimeProviderMock;

    private Fixture _fixture;
    private IMemoryCache _memoryCache;
    private ILogger<ExchangeCurrencyService> _logger;

    public ExchangeCurrencyServiceTest()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _logger = new NullLogger<ExchangeCurrencyService>();
    }

    [SetUp]
    public void Setup()
    {
        var user = _fixture.Create<User>();
        user.Id = 1;

        var transaction = _fixture.Create<Transaction>();
        transaction.User = user;
        transaction.UserId = user.Id;
        transaction.TransactionTime = DateTime.UtcNow;

        ServiceProvider serviceProvider = GetServiceProviderWithIMemoryCache();
        _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

        _mapperMock = new Mock<IMapper>();
        _appDbContextMock = new Mock<IApplicationDbContext>();
        _fixerApiServiceMock = new Mock<IFixerApiRequester>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _mapperMock.Setup(m => m.Map<Transaction>(It.IsAny<TransactionInfo>())).Returns(transaction);

        _appDbContextMock.Setup(context => context.Users).Returns(CreateDbSetMock(new List<User> { user }).Object);
        _appDbContextMock.Setup(context => context.Transactions).Returns(CreateDbSetMock(new List<Transaction> { transaction }).Object);
        _appDbContextMock.Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

        _fixerApiServiceMock.Setup(service => service.GetConvertResponse(It.IsAny<TransactionCreationRequest>()))
            .ReturnsAsync(It.IsAny<TransactionInfo>());

        _dateTimeProviderMock.Setup(provider => provider.GetCurrentDateTime()).Returns(DateTime.UtcNow);
    }

    private static ServiceProvider GetServiceProviderWithIMemoryCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();

        return services.BuildServiceProvider();
    }

    [Test, AutoData]
    public async Task PassCorrectData_CallCurrencyServiceApi_RecieveCorrectTransactionFromApi
        (TransactionCreationRequest request, TransactionInfo transaction)
    {
        transaction.Query.Amount = request.Amount;
        transaction.Query.From = (DomainCurrency)request.From;
        transaction.Query.To = (DomainCurrency)request.To;

        _fixerApiServiceMock.Setup(service => service.GetConvertResponse(request))
            .ReturnsAsync(transaction);

        var sut = new ExchangeCurrencyService(
            _fixerApiServiceMock.Object,
            _appDbContextMock.Object,
            _mapperMock.Object,
            _logger,
            _memoryCache,
            _dateTimeProviderMock.Object);

        //Act
        var result = await sut.ExchangeCurrency(request, 1);

        //Assert
        Assert.IsNotNull(result);
        Assert.That(request.Amount == result.Query.Amount);
        Assert.That((DomainCurrency)request.To == result.Query.To);
        Assert.That((DomainCurrency)request.From == result.Query.From);
        Assert.That(_memoryCache.TryGetValue(string.Concat(request.From, request.To), out var memoryCache));
    }

    [Test, AutoData]
    public async Task SetNumberOfRequestForUserMoreThenTenForLastHour_CallCurrencyService_RecieveNumberOfRequestExceededException
        (TransactionCreationRequest request)
    {
        //Arrange
        var transactions = _fixture.CreateMany<Transaction>(11).ToList();

        transactions.ForEach(t =>
        {
            t.UserId = 1;
            t.TransactionTime = DateTime.UtcNow;
        });

        _appDbContextMock.Setup(context => context.Transactions).Returns(CreateDbSetMock(transactions).Object);

        var sut = new ExchangeCurrencyService(
            _fixerApiServiceMock.Object,
            _appDbContextMock.Object,
            _mapperMock.Object,
            _logger,
            _memoryCache,
            _dateTimeProviderMock.Object);

        //Act && Assert
        var exception = Assert.ThrowsAsync<NumberOfRequestExceededException>
            (async () => await sut.ExchangeCurrency(request, 1));

        Assert.NotNull(exception);
        Assert.IsFalse(_memoryCache.TryGetValue(string.Concat(request.From, request.To), out var memoryCache));
    }

    [Test, AutoData]
    public async Task SetRequestToMemoryCache_CallServiceExchange_RetrieveDataFromCache
        (TransactionCreationRequest request, TransactionInfo transactionInfo)
    {
        //Arrange
        transactionInfo.Query.Amount = request.Amount;
        transactionInfo.Query.From = (DomainCurrency)request.From;
        transactionInfo.Query.To = (DomainCurrency)request.To;

        _memoryCache.Set(string.Concat(request.From, request.To), transactionInfo);

        var sut = new ExchangeCurrencyService(
            _fixerApiServiceMock.Object,
            _appDbContextMock.Object,
            _mapperMock.Object,
            _logger,
            _memoryCache,
            _dateTimeProviderMock.Object);

        //Act && Assert
        var transaction = await sut.ExchangeCurrency(request, 1);

        Assert.NotNull(transaction);
        Assert.That(_memoryCache.TryGetValue(string.Concat(request.From, request.To), out var memoryCache));
        Assert.That(transactionInfo == memoryCache);
        _fixerApiServiceMock.Verify(f => f.GetConvertResponse(request), Times.Never());
    }

    private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
    {
        var elementsAsQueryable = elements.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());

        return dbSetMock;
    }
}