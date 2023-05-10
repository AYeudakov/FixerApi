using MeDirect.CurrencyExchange.Options;

namespace MeDirect.CurrencyExchange.Extensions;

public static class ServiceHttpClientFactoryExtension
{
    public static IServiceCollection AddHttpClientForProvider(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        ProviderOptions providerOptions = new ProviderOptions(configuration);

        serviceCollection.AddHttpClient("fixerApi", client => 
        {
            client.BaseAddress = new Uri(providerOptions.Url);
            client.DefaultRequestHeaders
                .Add(nameof(providerOptions.ApiKey).ToLower(), providerOptions.ApiKey);
        });

        return serviceCollection;
    }
}