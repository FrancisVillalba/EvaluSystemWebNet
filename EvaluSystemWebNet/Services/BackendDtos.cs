namespace EvaluSystemWebNet.Services;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record LoginRequest(string Usuario, string Pass);

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    int UsuarioId,
    string Usuario,
    int? PersonaId,
    string? Persona);

public record ClienteDto(
    int Id,
    string? Nombre,
    string? Documento,
    string TipoDocumentoId,
    string TipoClienteId,
    string? Email,
    string? NroTelefono,
    string? Direccion,
    bool? Estado,
    ClienteDatosEnvioDto? DatosEnvio);

public record ClienteDatosEnvioDto(
    int Id,
    int ClienteId,
    int TransportadoraId,
    string? Transportadora,
    string NombreReceptor,
    string DocumentoReceptor,
    string TelefonoReceptor,
    int DepartamentoId,
    string? Departamento,
    int CiudadId,
    string? Ciudad,
    string Direccion,
    string? Observacion,
    bool Estado);

public record VentaImpresionCabDto(
    int Id,
    int ClienteId,
    string? Cliente,
    string FormaPagoId,
    string? FormaPago,
    decimal TotalVenta,
    string EstadoVentaId,
    string? EstadoVenta,
    int VendedorId,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    string? EstadoPagado,
    DateTime? FechaEntrega,
    string? ComprobantePago,
    string? ComprobantePagoNombre,
    string? Observacion,
    IEnumerable<VentaImpresionDetDto> Detalles);

public record VentaImpresionDetDto(
    int Id,
    int CabId,
    int ProductoId,
    string? Producto,
    int TipoMaquinaId,
    string? TipoMaquina,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PrecioExtra,
    decimal? PrecioTotal,
    string? ArchivoDisenio,
    string? ArchivoDisenioNombre,
    string? Observacion,
    string EstadoItem,
    bool? CheckImpresion);
