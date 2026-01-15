using PozitifWeb.OrderApp.Domain.Enums;

namespace PozitifWeb.OrderApp.Application.DTOs;

public record CreateOrderItemRequest(string ProductName, int Quantity, decimal UnitPrice);

public record CreateOrderRequest(int CustomerId, DateTime? OrderDate, List<CreateOrderItemRequest> Items);

public record CreateOrderResponse(int Id, decimal TotalAmount, OrderStatus Status);

public record OrderListDto(int Id, int CustomerId, DateTime OrderDate, OrderStatus Status, decimal TotalAmount);

public record OrderItemDto(string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderDetailDto(
    int Id,
    int CustomerId,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    List<OrderItemDto> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);