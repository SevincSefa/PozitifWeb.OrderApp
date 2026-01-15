namespace PozitifWeb.OrderApp.Application.DTOs;

public record CustomerListDto(int Id, string Name, string Email, DateTime CreatedDate);

public record CreateCustomerRequest(string Name, string Email);

public record CreateCustomerResponse(int Id);