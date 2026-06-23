using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public DashboardController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var pedidos = await _backendApiClient.GetAsync<PagedResponse<VentaImpresionCabDto>>(
            $"api/VentasImpresion?page={page}&pageSize={pageSize}",
            cancellationToken);

        if (pedidos is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener el dashboard desde EvaluSystemBack." });
        }

        var lista = pedidos.Items.ToList();
        var today = DateTime.Today;
        var loadedToday = lista.Count(x => x.FechaEntrega?.Date == today);
        var printed = lista.Count(x => IsPrinted(x.EstadoVenta));
        var delivered = lista.Count(x => IsDelivered(x.EstadoVenta));

        var dashboard = new DashboardView(
            new DashboardMetrics(
                loadedToday,
                printed,
                Math.Max(lista.Count - printed, 0),
                $"{delivered} / {lista.Count}"),
            lista
                .OrderByDescending(x => x.Id)
                .Select(ToDashboardOrder),
            pedidos.Page,
            pedidos.PageSize,
            pedidos.TotalItems,
            pedidos.TotalPages);

        return Ok(dashboard);
    }

    private static DashboardOrderView ToDashboardOrder(VentaImpresionCabDto pedido)
    {
        var detalles = pedido.Detalles.ToList();
        var firstDetail = detalles.FirstOrDefault();
        var meters = detalles.Sum(x => x.Cantidad);

        return new DashboardOrderView(
            pedido.Id.ToString(),
            pedido.Cliente ?? string.Empty,
            pedido.VendedorId.ToString(),
            firstDetail?.Producto ?? string.Empty,
            meters > 0 ? $"{meters:N2} m" : "0 m",
            pedido.EstadoVenta ?? pedido.EstadoVentaId,
            pedido.FechaEntrega?.ToString("yyyy-MM-dd") ?? string.Empty);
    }

    private static bool IsPrinted(string? estado)
    {
        return estado?.Contains("impres", StringComparison.OrdinalIgnoreCase) == true
            || estado?.Contains("entreg", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsDelivered(string? estado)
    {
        return estado?.Contains("entreg", StringComparison.OrdinalIgnoreCase) == true;
    }
}
