using PozitifWeb.OrderApp.Application.DTOs;

namespace PozitifWeb.OrderApp.Application.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct);

    Task<(IReadOnlyList<OrderListDto> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct);

    Task<OrderDetailDto> GetByIdAsync(int id, CancellationToken ct);

    Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken ct);
}