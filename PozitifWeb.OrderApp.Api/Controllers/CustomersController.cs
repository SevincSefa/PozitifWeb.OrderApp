using Microsoft.AspNetCore.Mvc;
using PozitifWeb.OrderApp.Application.DTOs;
using PozitifWeb.OrderApp.Application.Interfaces;

namespace PozitifWeb.OrderApp.Api.Controllers;

/// <summary>
/// Müşteri yönetimi endpoint'lerini sağlar.
/// </summary>
/// <remarks>
/// Bu controller; müşteri listeleme ve müşteri oluşturma işlemlerini yönetir.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    /// <summary>
    /// Tüm müşterileri listeler.
    /// </summary>
    /// <remarks>
    /// Müşteri kayıtlarını sistemde tutulduğu şekilde listeler.
    /// Liste genellikle servis tarafında oluşturulma tarihine göre sıralanır.
    /// </remarks>
    /// <param name="ct">İsteğin iptali için cancellation token.</param>
    /// <response code="200">Müşteriler başarıyla listelendi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await customerService.GetAllAsync(ct);
        return Ok(items);
    }

    /// <summary>
    /// Yeni müşteri oluşturur.
    /// </summary>
    /// <remarks>
    /// Sisteme yeni müşteri kaydı oluşturur.
    ///
    /// İş kuralları:
    /// - Email adresi benzersiz olmalıdır.
    ///
    /// Örnek Request:
    /// <code>
    /// {
    ///   "name": "Sefa Sevinc",
    ///   "email": "sefa@pozitifweb.com"
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
    /// <param name="request">Oluşturulacak müşterinin bilgileri.</param>
    /// <param name="ct">İsteğin iptali için cancellation token.</param>
    /// <response code="201">Müşteri başarıyla oluşturuldu.</response>
    /// <response code="400">Validasyon hatası oluştu.</response>
    /// <response code="409">Bu email ile kayıtlı müşteri zaten mevcut.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await customerService.CreateAsync(request, ct);

        // Not: ideal olarak CreatedAtAction GetById endpoint'ine yönlendirmelidir.
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
