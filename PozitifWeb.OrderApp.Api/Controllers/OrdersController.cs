using Microsoft.AspNetCore.Mvc;
using PozitifWeb.OrderApp.Api.Models;
using PozitifWeb.OrderApp.Application.DTOs;
using PozitifWeb.OrderApp.Application.Interfaces;

namespace PozitifWeb.OrderApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // GET /api/orders?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var (items, totalCount) = await orderService.GetPagedAsync(page, pageSize, ct);
        return Ok(new PagedResponse<OrderListDto>(items, page <= 0 ? 1 : page, pageSize, totalCount));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var dto = await orderService.GetByIdAsync(id, ct);
        return Ok(dto);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        await orderService.UpdateStatusAsync(id, request, ct);
        return NoContent();
    }
}
