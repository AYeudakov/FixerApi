#pragma warning disable CS8618
namespace CurrencyExchange.Options;

public class JwtOptions
{
	public JwtOptions(IConfiguration configuration)
	{
		configuration.GetRequiredSection(nameof(JwtOptions)).Bind(this);
	}

	public string Secret { get; set; }
}