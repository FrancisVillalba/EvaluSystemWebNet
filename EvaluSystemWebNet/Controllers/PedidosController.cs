using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private static readonly List<PedidoMock> Pedidos =
    [
        new(
            "DTF-1048",
            DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            "Urban Print Co.",
            "Camila",
            "Pendiente de impresion",
            "2026-06-22",
            "Transferencia",
            "Pendiente",
            "0",
            "",
            "",
            [new PedidoDetalleMock("DTF Textil por metro", "Epson DTF 60cm", "18", "80000", "", "YESSICA MORINIGO TEXTIL.png")]
        ),
        new(
            "UV-1047",
            DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            "Brava Store",
            "Martin",
            "Impreso",
            "2026-06-22",
            "Transferencia",
            "Parcial",
            "400000",
            "",
            "",
            [new PedidoDetalleMock("Sticker UV DTF", "UV DTF A3+", "9", "95000", "", "logo-brava.png")]
        ),
        new(
            "DTF-1046",
            DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            "Norte Uniformes",
            "Sofia",
            "Entregado",
            "2026-06-23",
            "Efectivo",
            "Pagado",
            "2120000",
            "",
            "",
            [new PedidoDetalleMock("DTF Textil por metro", "Epson DTF 60cm", "24", "82000", "152000", "uniformes-norte.png")]
        ),
        new(
            "UV-1045",
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)).ToString("yyyy-MM-dd"),
            "Mateo Accesorios",
            "Camila",
            "Cargado",
            "2026-06-22",
            "Tarjeta",
            "Pendiente",
            "0",
            "",
            "",
            [new PedidoDetalleMock("Sticker UV DTF", "UV DTF A3+", "6", "90000", "", "sticker-mateo.png")]
        )
    ];

    [HttpGet]
    public ActionResult<IEnumerable<PedidoMock>> GetAll()
    {
        return Ok(Pedidos);
    }

    [HttpGet("{id}")]
    public ActionResult<PedidoMock> GetById(string id)
    {
        var pedido = Pedidos.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        return pedido is null ? NotFound() : Ok(pedido);
    }
}

public record PedidoMock(
    string Id,
    string Date,
    string Client,
    string Seller,
    string Status,
    string Delivery,
    string PaymentMethod,
    string PaymentStatus,
    string PaidAmount,
    string ProofName,
    string Notes,
    List<PedidoDetalleMock> Details
);

public record PedidoDetalleMock(
    string Product,
    string Machine,
    string Quantity,
    string UnitPrice,
    string ExtraPrice,
    string DesignName
);
