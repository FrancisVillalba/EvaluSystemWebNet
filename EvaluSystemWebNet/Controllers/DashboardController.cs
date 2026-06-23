using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var dashboard = new
        {
            Metrics = new
            {
                LoadedToday = 60,
                Printed = 46,
                MissingPrint = 14,
                Delivered = "50 / 60"
            },
            RecentOrders = new[]
            {
                new { Id = "DTF-1048", Client = "Urban Print Co.", Seller = "Camila", Type = "DTF Textil", Meters = "18 m", Status = "Pendiente de impresion", Delivery = "Hoy 16:30" },
                new { Id = "UV-1047", Client = "Brava Store", Seller = "Martin", Type = "UV DTF", Meters = "9 m", Status = "En impresion", Delivery = "Hoy 18:00" },
                new { Id = "DTF-1046", Client = "Norte Uniformes", Seller = "Sofia", Type = "DTF Textil", Meters = "24 m", Status = "Diseno aprobado", Delivery = "Manana 10:00" },
                new { Id = "UV-1045", Client = "Mateo Accesorios", Seller = "Camila", Type = "UV DTF", Meters = "6 m", Status = "Pendiente de pago", Delivery = "Manana 15:00" }
            }
        };

        return Ok(dashboard);
    }
}
