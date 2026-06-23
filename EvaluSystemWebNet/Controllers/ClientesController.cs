using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<ClienteMock>> GetAll()
    {
        var clientes = new List<ClienteMock>
        {
            new(1, "Urban Print Co.", "80012345-1", "0981 220 440", "ventas@urbanprint.com", "Asuncion", false, "Activo"),
            new(2, "Brava Store", "80155220-7", "0972 880 112", "pedidos@bravastore.com", "San Lorenzo", false, "Activo"),
            new(3, "Casa Grafica", "80045090-2", "0984 610 722", "admin@casagrafica.com", "Luque", true, "Inactivo")
        };

        return Ok(clientes);
    }
}

public record ClienteMock(
    int Id,
    string Name,
    string Document,
    string Phone,
    string Email,
    string City,
    bool Carrier,
    string Status
);
