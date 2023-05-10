namespace CurrencyExchange.Application.Services;

public interface IDateTimeProvider
{
    public DateTime GetCurrentDateTime();
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentDateTime()
    {
        return DateTime.UtcNow;
    }
}