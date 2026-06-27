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
        [FromQuery] int? vendedorId = null,
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

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        var pedidosTask = _backendApiClient.GetAsync<PagedResponse<VentaImpresionCabDto>>(
            $"api/VentasImpresion?{string.Join("&", filters)}",
            cancellationToken);
        var opcionesTask = _backendApiClient.GetAsync<PedidoFormOptionsDto>("api/VentasImpresion/opciones", cancellationToken);

        await Task.WhenAll(pedidosTask, opcionesTask);

        var pedidos = await pedidosTask;

        if (pedidos is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener pedidos desde EvaluSystemBack." });
        }

        var vendedores = (await opcionesTask)?.Vendedores
            .ToDictionary(x => x.Id, x => x.Persona ?? x.NombreUsuario ?? $"Usuario {x.Id}")
            ?? new Dictionary<int, string>();

        return Ok(new PagedView<PedidoView>(
            pedidos.Items.Select(pedido => ToView(pedido, vendedores)),
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
        var options = await _backendApiClient.GetAsync<PedidoFormOptionsDto>("api/VentasImpresion/opciones", cancellationToken);
        return options is null
            ? StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudieron obtener opciones de pedidos desde EvaluSystemBack." })
            : Ok(options);
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
    public async Task<IActionResult> Delete(
        int id,
        [FromBody] EliminarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Observacion))
        {
            return BadRequest(new { message = "Debe agregar un comentario para eliminar el pedido." });
        }

        var result = await _backendApiClient.PutResultAsync<VentaImpresionCabDto>(
            $"api/VentasImpresion/{id}/marcar-eliminado",
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    private static PedidoView ToView(VentaImpresionCabDto pedido, IReadOnlyDictionary<int, string>? vendedores = null)
    {
        var date = pedido.FechaCreacion?.ToString("yyyy-MM-dd") ?? string.Empty;
        var delivery = pedido.FechaEntrega?.ToString("yyyy-MM-dd") ?? string.Empty;
        var vendedor = !string.IsNullOrWhiteSpace(pedido.Vendedor)
            ? pedido.Vendedor
            : vendedores?.GetValueOrDefault(pedido.VendedorId) ?? $"Usuario {pedido.VendedorId}";

        return new PedidoView(
            pedido.Id.ToString(),
            pedido.ClienteId,
            pedido.FormaPagoId,
            pedido.VendedorId,
            pedido.EstadoVentaId,
            pedido.EstadoPagadoId,
            date,
            pedido.Cliente ?? string.Empty,
            vendedor,
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
