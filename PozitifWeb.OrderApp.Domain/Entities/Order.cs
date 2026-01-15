using PozitifWeb.OrderApp.Domain.Enums;

namespace PozitifWeb.OrderApp.Domain.Entities;

public class Order : BaseEntity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}