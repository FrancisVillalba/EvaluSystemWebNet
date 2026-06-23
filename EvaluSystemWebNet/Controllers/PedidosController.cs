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
    public async Task<ActionResult<IEnumerable<PedidoView>>> GetAll(CancellationToken cancellationToken)
    {
        var pedidos = await _backendApiClient.GetAsync<IEnumerable<VentaImpresionCabDto>>("api/VentasImpresion", cancellationToken);

        if (pedidos is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener pedidos desde EvaluSystemBack." });
        }

        return Ok(pedidos.Select(ToView));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoView>> GetById(int id, CancellationToken cancellationToken)
    {
        var pedido = await _backendApiClient.GetAsync<VentaImpresionCabDto>($"api/VentasImpresion/{id}", cancellationToken);
        return pedido is null ? NotFound() : Ok(ToView(pedido));
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
