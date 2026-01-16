using Microsoft.AspNetCore.Mvc;
using PozitifWeb.OrderApp.Api.Models;
using PozitifWeb.OrderApp.Application.DTOs;
using PozitifWeb.OrderApp.Application.Interfaces;

namespace PozitifWeb.OrderApp.Api.Controllers;

/// <summary>
/// Sipariş yönetimi endpoint'lerini sağlar.
/// </summary>
/// <remarks>
/// Bu controller; sipariş oluşturma, sayfalı listeleme, detay görüntüleme ve sipariş durum güncelleme işlemlerini yönetir.
/// </remarks>
[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>
    /// Yeni sipariş oluşturur.
    /// </summary>
    /// <remarks>
    /// Sisteme yeni bir sipariş kaydı oluşturur ve oluşturulan kaynağın detayına yönlendiren 201 (Created) döner.
    ///
    /// Örnek Request:
    /// <code>
    /// {
    ///   "customerId": 1,
    ///   "items": [
    ///     { "productId": 10, "quantity": 2 },
    ///     { "productId": 11, "quantity": 1 }
    ///   ]
    /// }
    /// </code>
    ///
    /// Örnek Response:
    /// <code>
    /// {
    ///   "id": 123
    /// }
    /// </code>
    /// </remarks>
    /// <param name="request">Oluşturulacak sipariş bilgileri.</param>
    /// <param name="ct">İsteğin iptali için cancellation token.</param>
    /// <response code="201">Sipariş başarıyla oluşturuldu.</response>
    /// <response code="400">Validasyon hatası oluştu.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Siparişleri sayfalı (paged) olarak listeler.
    /// </summary>
    /// <remarks>
    /// Varsayılan değerler: page = 1, pageSize = 20
    ///
    /// Örnek:
    /// <code>
    /// GET /api/orders?page=1&amp;pageSize=20
    /// </code>
    /// </remarks>
    /// <param name="page">Sayfa numarası (1 tabanlı).</param>
    /// <param name="pageSize">Sayfa başına kayıt adedi.</param>
    /// <param name="ct">İsteğin iptali için cancellation token.</param>
    /// <response code="200">Sayfalı sipariş listesi döner.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await orderService.GetPagedAsync(page, pageSize, ct);
        return Ok(new PagedResponse<OrderListDto>(items, page <= 0 ? 1 : page, pageSize, totalCount));
    }

    /// <summary>
    /// Sipariş detayını getirir.
    /// </summary>
    /// <remarks>
    /// Belirtilen sipariş ID'sine ait detay bilgilerini döner.
    /// </remarks>
    /// <param name="id">Sipariş ID.</param>
    /// <param name="ct">İsteğin iptali için cancellation token.</param>
    /// <response code="200">Sipariş detayı döner.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var dto = await orderService.GetByIdAsync(id, ct);
        return Ok(dto);
    }

    /// <summary>
    /// Sipariş durumunu günceller.
    /// </summary>
    /// <remarks>
    /// Mevcut siparişin durumunu değiştirir. Başarılı olduğunda 204 (No Content) döner.
    ///
    /// Örnek Request:
    /// <code>
    /// {
    ///   "status": "Preparing"
    /// }
    /// </code>
    /// </remarks>
    /// <param name="id">Durumu güncellenecek sipariş ID.</param>
    /// <param name="request">Yeni durum bilgisi.</param>
    /// <param name="ct">İsteğin iptali için cancellation token.</param>
    /// <response code="204">Sipariş durumu başarıyla güncellendi.</response>
    /// <response code="400">Validasyon hatası oluştu.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
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
