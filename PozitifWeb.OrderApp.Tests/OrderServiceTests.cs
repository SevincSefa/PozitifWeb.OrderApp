using PozitifWeb.OrderApp.Application.DTOs;
using PozitifWeb.OrderApp.Application.Expections;
using PozitifWeb.OrderApp.Application.Services;
using PozitifWeb.OrderApp.Domain.Entities;
using PozitifWeb.OrderApp.Domain.Enums;
using Xunit;

namespace PozitifWeb.OrderApp.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Throw_When_ItemsEmpty()
    {
        await using var db = TestDbFactory.CreateDbContext();

        db.Customers.Add(new Customer { Name = "Test", Email = "test@test.com" });
        await db.SaveChangesAsync();

        var service = new OrderService(db);

        var req = new CreateOrderRequest(
            CustomerId: 1,
            OrderDate: DateTime.UtcNow,
            Items: new List<CreateOrderItemRequest>()
        );

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Should_Calculate_TotalAmount()
    {
        await using var db = TestDbFactory.CreateDbContext();

        db.Customers.Add(new Customer { Name = "Test", Email = "test@test.com" });
        await db.SaveChangesAsync();

        var service = new OrderService(db);

        var req = new CreateOrderRequest(
            CustomerId: 1,
            OrderDate: new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            Items: new List<CreateOrderItemRequest>
            {
                new("Kalem", 10, 20m),   // 200
                new("Defter", 2, 50m)   // 100
            }
        );

        var res = await service.CreateAsync(req, CancellationToken.None);

        Assert.Equal(300m, res.TotalAmount);
        Assert.Equal(OrderStatus.Pending, res.Status);
        Assert.True(res.Id > 0);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_CustomerNotFound()
    {
        await using var db = TestDbFactory.CreateDbContext();
        var service = new OrderService(db);

        var req = new CreateOrderRequest(
            CustomerId: 999,
            OrderDate: DateTime.UtcNow,
            Items: new List<CreateOrderItemRequest> { new("Kalem", 1, 10m) }
        );

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_DailyLimitExceeded_Throws()
    {
        await using var db = TestDbFactory.CreateDbContext();

        db.Customers.Add(new Customer { Name = "Test", Email = "test@test.com" });
        await db.SaveChangesAsync();

        var day = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < 5; i++)
        {
            var order = new Order
            {
                CustomerId = 1,
                OrderDate = day.AddHours(i),
                Status = OrderStatus.Pending,
                TotalAmount = 10m
            };

            order.Items.Add(new OrderItem { ProductName = "X", Quantity = 1, UnitPrice = 10m });

            db.Orders.Add(order);
        }

        await db.SaveChangesAsync();

        var service = new OrderService(db);

        var req = new CreateOrderRequest(
            CustomerId: 1,
            OrderDate: day,
            Items: new List<CreateOrderItemRequest> { new("Kalem", 1, 10m) }
        );

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Throw_When_Completed()
    {
        await using var db = TestDbFactory.CreateDbContext();

        db.Customers.Add(new Customer { Name = "Test", Email = "test@test.com" });
        await db.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = 1,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Completed,
            TotalAmount = 10m
        };
        order.Items.Add(new OrderItem { ProductName = "X", Quantity = 1, UnitPrice = 10m });

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var service = new OrderService(db);

        var req = new UpdateOrderStatusRequest(OrderStatus.Cancelled);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateStatusAsync(order.Id, req, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_PendingToCompleted_Updates()
    {
        await using var db = TestDbFactory.CreateDbContext();

        db.Customers.Add(new Customer { Name = "Test", Email = "test@test.com" });
        await db.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = 1,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 10m
        };
        order.Items.Add(new OrderItem { ProductName = "X", Quantity = 1, UnitPrice = 10m });

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var service = new OrderService(db);

        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest(OrderStatus.Completed), CancellationToken.None);

        var updated = await db.Orders.FindAsync(order.Id);
        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Completed, updated!.Status);
    }
}
