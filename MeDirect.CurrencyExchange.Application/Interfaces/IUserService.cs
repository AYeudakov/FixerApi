using MeDirect.CurrencyExchange.Application.Entities;
using MeDirect.CurrencyExchange.Application.Requests;

namespace MeDirect.CurrencyExchange.Application.Interfaces;

public interface IUserService
{
    public Task<string> LoginAsync(LoginRequest request, string secret);
    public Task<User> RegisterAsync(UserRequest request, CancellationToken cancellationToken);
}