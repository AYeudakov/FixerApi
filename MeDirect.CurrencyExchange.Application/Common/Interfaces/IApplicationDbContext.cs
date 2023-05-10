using MeDirect.CurrencyExchange.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeDirect.CurrencyExchange.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<Transaction> Transactions { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}