using PozitifWeb.OrderApp.Application.DTOs;

namespace PozitifWeb.OrderApp.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListDto>> GetAllAsync(CancellationToken ct);
    Task<CreateCustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct);
}