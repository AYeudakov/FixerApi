using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Requests;

namespace CurrencyExchange.Application.Interfaces;

public interface IUserService
{
    public Task<string> LoginAsync(LoginRequest request, string secret);
    public Task<User> RegisterAsync(UserRequest request, CancellationToken cancellationToken);
}