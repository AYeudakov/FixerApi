namespace CurrencyExchange.Domain.Exceptions;

public class NumberOfRequestExceededException : Exception
{
	public NumberOfRequestExceededException(string message) : base(message) { }
}