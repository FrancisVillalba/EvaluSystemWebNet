namespace EvaluSystemWebNet.Services;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record DashboardSummaryDto(
    int TotalPedidos,
    int PedidosCargados,
    int PedidosImpresos,
    int PedidosPendientesImpresion,
    int PedidosEntregados,
    IEnumerable<DashboardMachineDto> PedidosPorMaquina,
    IEnumerable<DashboardMoneyDto> PendientesPago,
    IEnumerable<DashboardSellerDto> MejoresVendedores);

public record DashboardMachineDto(string Nombre, int Cantidad);

public record DashboardMoneyDto(string Nombre, decimal Monto);

public record DashboardSellerDto(string Nombre, int Cantidad);

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

public record CatalogStringDto(string Id, string? Nombre, bool? Estado);

public record UsuarioDto(int Id, string? NombreUsuario, int? PersonaId, string? Persona, bool? Estado);

public record ProductoDto(int Id, string Nombre, decimal PrecioBase, decimal? Comision, int? MaquinaId, string? Maquina, bool Estado);

public record TipoMaquinaDto(int Id, string Nombre, bool Estado);

public record PedidoFormOptionsDto(
    IEnumerable<ClienteDto> Clientes,
    IEnumerable<CatalogStringDto> FormasPago,
    IEnumerable<UsuarioDto> Vendedores,
    IEnumerable<CatalogStringDto> EstadosPago,
    IEnumerable<ProductoDto> Productos,
    IEnumerable<TipoMaquinaDto> Maquinas);

public record VentaImpresionCompletaRequest(
    int ClienteId,
    string FormaPagoId,
    int VendedorId,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    DateTime? FechaEntrega,
    string? ComprobantePago,
    string? ComprobantePagoNombre,
    string? Observacion,
    string? EstadoVentaId,
    IEnumerable<VentaImpresionDetalleCreateRequest> Detalles);

public record VentaImpresionDetalleCreateRequest(
    int ProductoId,
    int TipoMaquinaId,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PrecioExtra,
    string? ArchivoDisenio,
    string? ArchivoDisenioNombre,
    string? Observacion,
    string? EstadoItem,
    bool? CheckImpresion);

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
