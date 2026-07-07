using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public DeliveryController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<DeliveryPedidoDto>>> GetDisponibles(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryPedidoDto>>("api/Delivery/disponibles", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar pedidos para delivery." });
    }

    [HttpGet("transportadora")]
    public async Task<ActionResult<IEnumerable<DeliveryPedidoDto>>> GetTransportadora(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryPedidoDto>>("api/Delivery/transportadora", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar pedidos para transportadora." });
    }

    [HttpGet("motobolt")]
    public async Task<ActionResult<IEnumerable<DeliveryPedidoDto>>> GetMotobolt(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryPedidoDto>>("api/Delivery/motobolt", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar pedidos para Motobolt." });
    }

    [HttpGet("retiro-local")]
    public async Task<ActionResult<IEnumerable<DeliveryPedidoDto>>> GetRetiroLocal(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryPedidoDto>>("api/Delivery/retiro-local", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar pedidos para retiro del local." });
    }

    [HttpGet("mis-pedidos")]
    public async Task<ActionResult<IEnumerable<DeliveryPedidoDto>>> GetMisPedidos(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryPedidoDto>>("api/Delivery/mis-pedidos", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar tus pedidos de delivery." });
    }

    [HttpGet("mis-rutas")]
    public async Task<ActionResult<IEnumerable<DeliveryRutaDto>>> GetMisRutas(
        [FromQuery] string? fechaDesde,
        [FromQuery] string? fechaHasta,
        CancellationToken cancellationToken)
    {
        var endpoint = $"api/Delivery/mis-rutas?{BuildDeliveryFilters(fechaDesde, fechaHasta, null)}";
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryRutaDto>>(endpoint, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar tus lotes de delivery." });
    }

    [HttpGet("entregas")]
    public async Task<ActionResult<IEnumerable<DeliveryPedidoDto>>> GetEntregas(
        [FromQuery] string? fechaDesde,
        [FromQuery] string? fechaHasta,
        [FromQuery] int? deliveryId,
        CancellationToken cancellationToken)
    {
        var endpoint = $"api/Delivery/entregas?{BuildDeliveryFilters(fechaDesde, fechaHasta, deliveryId)}";
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryPedidoDto>>(endpoint, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar las entregas." });
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<IEnumerable<DeliveryResumenDto>>> GetResumen(
        [FromQuery] string? fechaDesde,
        [FromQuery] string? fechaHasta,
        CancellationToken cancellationToken)
    {
        var endpoint = $"api/Delivery/resumen?{BuildDeliveryFilters(fechaDesde, fechaHasta, null)}";
        var result = await _backendApiClient.GetResultAsync<IEnumerable<DeliveryResumenDto>>(endpoint, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo cargar el resumen por delivery." });
    }

    [HttpPost("{id:int}/tomar")]
    public async Task<ActionResult<DeliveryPedidoDto>> TomarPedido(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<DeliveryPedidoDto>($"api/Delivery/{id}/tomar", new { }, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo tomar el pedido." });
    }

    [HttpPost("{id:int}/marcar-enviado")]
    public async Task<ActionResult<DeliveryPedidoDto>> MarcarEnviado(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<DeliveryPedidoDto>($"api/Delivery/{id}/marcar-enviado", new { }, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo marcar como enviado." });
    }

    [HttpPost("{id:int}/quitar")]
    public async Task<IActionResult> QuitarPedidoTomado(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<object>($"api/Delivery/{id}/quitar", new { }, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo quitar el pedido." });
    }

    [HttpPost("rutas/generar")]
    public async Task<ActionResult<DeliveryRutaDto>> GenerarRuta(
        [FromQuery] string? fechaDesde,
        [FromQuery] string? fechaHasta,
        [FromQuery] string? cliente,
        CancellationToken cancellationToken)
    {
        var query = BuildDeliveryFilters(fechaDesde, fechaHasta, null);
        if (!string.IsNullOrWhiteSpace(cliente))
        {
            query = string.IsNullOrWhiteSpace(query)
                ? $"cliente={Uri.EscapeDataString(cliente)}"
                : $"{query}&cliente={Uri.EscapeDataString(cliente)}";
        }

        var endpoint = string.IsNullOrWhiteSpace(query)
            ? "api/Delivery/rutas/generar"
            : $"api/Delivery/rutas/generar?{query}";
        var result = await _backendApiClient.PostResultAsync<DeliveryRutaDto>(endpoint, new { }, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo generar el lote de ruta." });
    }

    [HttpGet("reporte-ruta/pdf")]
    public async Task<IActionResult> DescargarRutaPdf([FromQuery] string? fechaDesde, [FromQuery] string? fechaHasta, CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(fechaDesde))
        {
            query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde)}");
        }

        if (!string.IsNullOrWhiteSpace(fechaHasta))
        {
            query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta)}");
        }

        var endpoint = query.Count == 0
            ? "api/Delivery/reporte-ruta/pdf"
            : $"api/Delivery/reporte-ruta/pdf?{string.Join("&", query)}";
        var file = await _backendApiClient.GetAsync<ExcelFileDto>(endpoint, cancellationToken);

        if (file is null || string.IsNullOrWhiteSpace(file.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo preparar el reporte de ruta." });
        }

        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    [HttpGet("rutas/{id:int}/pdf")]
    public async Task<IActionResult> DescargarRutaLotePdf(int id, CancellationToken cancellationToken)
    {
        var file = await _backendApiClient.GetAsync<ExcelFileDto>($"api/Delivery/rutas/{id}/pdf", cancellationToken);

        if (file is null || string.IsNullOrWhiteSpace(file.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo preparar el reporte de ruta." });
        }

        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    [HttpGet("{id:int}/transportadora-etiqueta/pdf")]
    public async Task<IActionResult> DescargarEtiquetaTransportadora(int id, CancellationToken cancellationToken)
    {
        var file = await _backendApiClient.GetAsync<ExcelFileDto>($"api/Delivery/{id}/transportadora-etiqueta/pdf", cancellationToken);

        if (file is null || string.IsNullOrWhiteSpace(file.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo preparar la etiqueta de transportadora." });
        }

        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    private static string BuildDeliveryFilters(string? fechaDesde, string? fechaHasta, int? deliveryId)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(fechaDesde))
        {
            query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde)}");
        }

        if (!string.IsNullOrWhiteSpace(fechaHasta))
        {
            query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta)}");
        }

        if (deliveryId.HasValue)
        {
            query.Add($"deliveryId={deliveryId.Value}");
        }

        return string.Join("&", query);
    }
}
