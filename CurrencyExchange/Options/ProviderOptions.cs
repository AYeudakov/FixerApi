#pragma warning disable CS8618
namespace CurrencyExchange.Options;

public class ProviderOptions
{
	public ProviderOptions(IConfiguration configuration)
	{
		configuration.GetRequiredSection(nameof(ProviderOptions)).Bind(this);
	}

	public string Url { get; set; }
	public string ApiKey { get; set; }
}