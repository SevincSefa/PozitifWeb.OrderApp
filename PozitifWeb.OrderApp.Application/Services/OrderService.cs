using Microsoft.EntityFrameworkCore;
using PozitifWeb.OrderApp.Application.DTOs;
using PozitifWeb.OrderApp.Application.Expections;
using PozitifWeb.OrderApp.Application.Interfaces;
using PozitifWeb.OrderApp.Domain.Entities;
using PozitifWeb.OrderApp.Domain.Enums;

namespace PozitifWeb.OrderApp.Application.Services;

public class OrderService(IAppDbContext db) : IOrderService
{
    public async Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {

        if (request.Items is null || request.Items.Count < 1)
            throw new BusinessRuleException("Bir sipariş en az 1 adet sipariş kalemi içermelidir.");

        var customerExists = await db.Customers.AnyAsync(x => x.Id == request.CustomerId, ct);
        if (!customerExists)
            throw new NotFoundException("Müşteri bulunamadı.");

        var orderDate = request.OrderDate ?? DateTime.UtcNow;
        var dayStart = orderDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var count = await db.Orders.CountAsync(o =>
            o.CustomerId == request.CustomerId &&
            o.OrderDate >= dayStart &&
            o.OrderDate < dayEnd, ct);

        if (count >= 5)
            throw new BusinessRuleException("Aynı müşteri aynı gün içerisinde en fazla 5 sipariş oluşturabilir.");

        // Test için gerekli IsRelational()
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = orderDate,
            Status = OrderStatus.Pending
        };

        foreach (var i in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            });
        }

        // TotalAmount, OrderItems üzerinden hesaplanması için;
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        if (tx is not null)
            await tx.CommitAsync(ct);

        return new CreateOrderResponse(order.Id, order.TotalAmount, order.Status);
    }

    public async Task<(IReadOnlyList<OrderListDto> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = db.Orders.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListDto(
                o.Id,
                o.CustomerId,
                o.OrderDate,
                o.Status,
                o.TotalAmount))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<OrderDetailDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var dto = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new OrderDetailDto(
                o.Id,
                o.CustomerId,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.Items.Select(i => new OrderItemDto(
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException("Sipariş bulunamadı.");
    }

    public async Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            throw new NotFoundException("Sipariş bulunamadı.");

        if (order.Status == OrderStatus.Cancelled)
            throw new BusinessRuleException("İptal edildi. durumundaki siparişler güncellenemez.");

        if (order.Status == OrderStatus.Completed)
            throw new BusinessRuleException("Tamamlandı durumundaki siparişlerde değişiklik yapılamaz.");

        if (order.Status == OrderStatus.Pending)
        {
            if (request.Status is not (OrderStatus.Completed or OrderStatus.Cancelled))
                throw new BusinessRuleException("Gönderiliyor durumundaki sipariş sadece Tamamlandı veya İptal Edildi yapılabilir.");

            order.Status = request.Status;
            await db.SaveChangesAsync(ct);
            return;
        }

        throw new BusinessRuleException("Geçersiz status değişimi.");
    }
}
