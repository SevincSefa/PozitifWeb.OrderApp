using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PozitifWeb.OrderApp.Domain.Entities;

namespace PozitifWeb.OrderApp.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    DatabaseFacade Database { get; } 
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}