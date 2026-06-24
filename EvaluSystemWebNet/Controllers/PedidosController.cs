using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public PedidosController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<ActionResult<PagedView<PedidoView>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? estadoVentaId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("yyyy-MM-dd"))}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={Uri.EscapeDataString(dateTo.Value.ToString("yyyy-MM-dd"))}");
        }

        if (clienteId.HasValue)
        {
            filters.Add($"clienteId={clienteId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(estadoVentaId))
        {
            filters.Add($"estadoVentaId={Uri.EscapeDataString(estadoVentaId)}");
        }

        var pedidos = await _backendApiClient.GetAsync<PagedResponse<VentaImpresionCabDto>>(
            $"api/VentasImpresion?{string.Join("&", filters)}",
            cancellationToken);

        if (pedidos is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener pedidos desde EvaluSystemBack." });
        }

        return Ok(new PagedView<PedidoView>(
            pedidos.Items.Select(ToView),
            pedidos.Page,
            pedidos.PageSize,
            pedidos.TotalItems,
            pedidos.TotalPages));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoView>> GetById(int id, CancellationToken cancellationToken)
    {
        var pedido = await _backendApiClient.GetAsync<VentaImpresionCabDto>($"api/VentasImpresion/{id}", cancellationToken);
        return pedido is null ? NotFound() : Ok(ToView(pedido));
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<PedidoFormOptionsDto>> GetOptions(CancellationToken cancellationToken)
    {
        var clientesTask = _backendApiClient.GetAsync<PagedResponse<ClienteDto>>("api/Clientes?page=1&pageSize=100", cancellationToken);
        var formasPagoTask = _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/FormasPago", cancellationToken);
        var vendedoresTask = _backendApiClient.GetAsync<IEnumerable<UsuarioDto>>("api/Usuarios", cancellationToken);
        var estadosPagoTask = _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/EstadosPago", cancellationToken);
        var estadosVentaTask = _backendApiClient.GetAsync<IEnumerable<EstadoVentaOptionDto>>("api/EstadosVenta", cancellationToken);
        var productosTask = _backendApiClient.GetAsync<IEnumerable<ProductoDto>>("api/Productos", cancellationToken);
        var maquinasTask = _backendApiClient.GetAsync<IEnumerable<TipoMaquinaDto>>("api/TiposMaquina", cancellationToken);

        await Task.WhenAll(clientesTask, formasPagoTask, vendedoresTask, estadosPagoTask, estadosVentaTask, productosTask, maquinasTask);

        var clientes = (await clientesTask)?.Items ?? [];
        var usuarioActualId = HttpContext.Session.GetInt32("BackendUserId");

        return Ok(new PedidoFormOptionsDto(
            clientes.Where(x => x.Estado != false),
            (await formasPagoTask ?? []).Where(x => x.Estado != false),
            (await vendedoresTask ?? []).Where(x => x.Estado != false),
            (await estadosPagoTask ?? []).Where(x => x.Estado != false),
            (await estadosVentaTask ?? []).Where(x => string.Equals(x.Estado, "A", StringComparison.OrdinalIgnoreCase)),
            (await productosTask ?? []).Where(x => x.Estado),
            (await maquinasTask ?? []).Where(x => x.Estado),
            usuarioActualId));
    }

    [HttpPost]
    public async Task<ActionResult<VentaImpresionCabDto>> Create(
        [FromBody] VentaImpresionCompletaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<VentaImpresionCabDto>(
            "api/VentasImpresion/completa",
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VentaImpresionCabDto>> Update(
        int id,
        [FromBody] VentaImpresionCompletaUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PutResultAsync<VentaImpresionCabDto>(
            $"api/VentasImpresion/completa/{id}",
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _backendApiClient.DeleteAsync($"api/VentasImpresion/{id}", cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status502BadGateway);
    }

    private static PedidoView ToView(VentaImpresionCabDto pedido)
    {
        var delivery = pedido.FechaEntrega?.ToString("yyyy-MM-dd") ?? string.Empty;

        return new PedidoView(
            pedido.Id.ToString(),
            pedido.ClienteId,
            pedido.FormaPagoId,
            pedido.VendedorId,
            pedido.EstadoVentaId,
            pedido.EstadoPagadoId,
            delivery,
            pedido.Cliente ?? string.Empty,
            pedido.VendedorId.ToString(),
            pedido.EstadoVenta ?? pedido.EstadoVentaId,
            delivery,
            pedido.FormaPago ?? pedido.FormaPagoId,
            pedido.EstadoPagado ?? pedido.EstadoPagadoId ?? "Pendiente",
            pedido.MontoPagado?.ToString("N0") ?? "0",
            pedido.ComprobantePagoNombre ?? string.Empty,
            pedido.Observacion ?? string.Empty,
            pedido.Detalles.Select(ToDetailView).ToList());
    }

    private static PedidoDetalleView ToDetailView(VentaImpresionDetDto detalle)
    {
        return new PedidoDetalleView(
            detalle.Id,
            detalle.ProductoId,
            detalle.TipoMaquinaId,
            detalle.Producto ?? string.Empty,
            detalle.TipoMaquina ?? string.Empty,
            detalle.Cantidad.ToString("N2"),
            detalle.PrecioUnitario.ToString("N0"),
            detalle.PrecioExtra?.ToString("N0") ?? string.Empty,
            detalle.ArchivoDisenioNombre ?? string.Empty);
    }
}
