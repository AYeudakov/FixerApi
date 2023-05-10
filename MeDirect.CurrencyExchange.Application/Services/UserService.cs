using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyExchange.Application.Common.Interfaces;
using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Requests;
using CurrencyExchange.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyExchange.Application.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IApplicationDbContext _applicationDbContext;

    public UserService(
        ILogger<UserService> logger,
        IApplicationDbContext applicationDbContext)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<string> LoginAsync(LoginRequest request, string secret)
    {
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(user => request.Email == user.Email);
        
        if (user == null)
        {
            string message = $"User with email:{request.Email} was not found";
            _logger.LogWarning(message);

            throw new UserNotFoundException(message);
        }
        
        return GenerateToken(user, secret);
    }

    private string GenerateToken(User request, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, request.Email),
            new("userId", request.Id.ToString()),
        };

        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(7)),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescription);
        
        return tokenHandler.WriteToken(token);
    }

    public async Task<User> RegisterAsync(UserRequest request, CancellationToken cancellationToken)
    {
        if (_applicationDbContext.Users.Any(user => user.Email == request.Email))
        {
            string message = $"User with email:{request.Email} was not found";
            _logger.LogWarning(message);
            
            throw new UserNotFoundException(message);
        }

        var user = new User
        {
            Id = request.Id,
            Email = request.Email
        };
        
        await _applicationDbContext.Users.AddAsync(user, cancellationToken);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}