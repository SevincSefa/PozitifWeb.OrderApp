namespace PozitifWeb.OrderApp.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Order? Order { get; set; }
}