using Microsoft.EntityFrameworkCore;
using PozitifWeb.OrderApp.Application.DTOs;
using PozitifWeb.OrderApp.Application.Expections;
using PozitifWeb.OrderApp.Application.Interfaces;
using PozitifWeb.OrderApp.Domain.Entities;

namespace PozitifWeb.OrderApp.Application.Services;

public class CustomerService(IAppDbContext db) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerListDto>> GetAllAsync(CancellationToken ct)
    {
        return await db.Customers
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new CustomerListDto(x.Id, x.Name, x.Email, x.CreatedDate))
            .ToListAsync(ct);
    }

    public async Task<CreateCustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();

        var exists = await db.Customers.AnyAsync(x => x.Email == email, ct);
        if (exists)
            throw new BusinessRuleException("Bu email ile kayıtlı müşteri zaten mevcut.");

        var customer = new Customer
        {
            Name = request.Name,
            Email = email
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);

        return new CreateCustomerResponse(customer.Id);
    }
}