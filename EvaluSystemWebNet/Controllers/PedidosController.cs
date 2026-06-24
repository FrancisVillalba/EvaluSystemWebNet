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
        CancellationToken cancellationToken = default)
    {
        var pedidos = await _backendApiClient.GetAsync<PagedResponse<VentaImpresionCabDto>>(
            $"api/VentasImpresion?page={page}&pageSize={pageSize}",
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
        var productosTask = _backendApiClient.GetAsync<IEnumerable<ProductoDto>>("api/Productos", cancellationToken);
        var maquinasTask = _backendApiClient.GetAsync<IEnumerable<TipoMaquinaDto>>("api/TiposMaquina", cancellationToken);

        await Task.WhenAll(clientesTask, formasPagoTask, vendedoresTask, estadosPagoTask, productosTask, maquinasTask);

        var clientes = (await clientesTask)?.Items ?? [];

        return Ok(new PedidoFormOptionsDto(
            clientes.Where(x => x.Estado != false),
            (await formasPagoTask ?? []).Where(x => x.Estado != false),
            (await vendedoresTask ?? []).Where(x => x.Estado != false),
            (await estadosPagoTask ?? []).Where(x => x.Estado != false),
            (await productosTask ?? []).Where(x => x.Estado),
            (await maquinasTask ?? []).Where(x => x.Estado)));
    }

    [HttpPost]
    public async Task<ActionResult<VentaImpresionCabDto>> Create(
        [FromBody] VentaImpresionCompletaRequest request,
        CancellationToken cancellationToken)
    {
        var venta = await _backendApiClient.PostAsync<VentaImpresionCabDto>(
            "api/VentasImpresion/completa",
            request,
            cancellationToken);

        if (venta is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo guardar la venta en EvaluSystemBack." });
        }

        return Ok(venta);
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
            detalle.Producto ?? string.Empty,
            detalle.TipoMaquina ?? string.Empty,
            detalle.Cantidad.ToString("N2"),
            detalle.PrecioUnitario.ToString("N0"),
            detalle.PrecioExtra?.ToString("N0") ?? string.Empty,
            detalle.ArchivoDisenioNombre ?? string.Empty);
    }
}
