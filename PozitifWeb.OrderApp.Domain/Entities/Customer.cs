namespace PozitifWeb.OrderApp.Domain.Entities;

public class Customer : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}