using CurrencyExchange.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CurrencyExchange.Extensions;

public static class AuthenticationServiceExtension
{
	public static IServiceCollection AddJwtBarrier(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		JwtOptions providerOptions = new JwtOptions(configuration);

		serviceCollection.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				IssuerSigningKey = new SymmetricSecurityKey
					(Encoding.UTF8.GetBytes(providerOptions.Secret)),
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true
			};
		});

		return serviceCollection;
	}
}