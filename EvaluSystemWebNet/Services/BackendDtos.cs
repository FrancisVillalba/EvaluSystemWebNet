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

public record DashboardSellerDto(string Nombre, decimal Monto);

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

public record ClienteRequest(
    string? Nombre,
    string? Documento,
    string TipoDocumentoId,
    string TipoClienteId,
    string? Email,
    string? NroTelefono,
    string? Direccion,
    bool? Estado);

public record ClienteDatosEnvioRequest(
    int ClienteId,
    int TransportadoraId,
    string NombreReceptor,
    string DocumentoReceptor,
    string TelefonoReceptor,
    int DepartamentoId,
    int CiudadId,
    string Direccion,
    string? Observacion,
    bool Estado);

public record CatalogStringDto(string Id, string? Nombre, bool? Estado);

public record DepartamentoDto(int Id, string Nombre, bool Estado);

public record CiudadDto(int Id, int DepartamentoId, string? Departamento, int CodigoDistrito, string Nombre, bool Estado);

public record TransportadoraDto(int Id, string Nombre, string? Telefono, string? Direccion, string? Observacion, bool Estado);

public record ClienteOptionsDto(
    IEnumerable<CatalogStringDto> TiposDocumento,
    IEnumerable<CatalogStringDto> TiposCliente,
    IEnumerable<TransportadoraDto> Transportadoras,
    IEnumerable<DepartamentoDto> Departamentos,
    IEnumerable<CiudadDto> Ciudades);

public record EstadoVentaOptionDto(string Id, string? Nombre, string? Estado, int? NumeroFlujo);

public record UsuarioDto(int Id, string? NombreUsuario, int? PersonaId, string? Persona, int? PerfilId, string? Perfil, bool? Estado);

public record ProductoDto(int Id, string Nombre, decimal PrecioBase, decimal? Comision, int? MaquinaId, string? Maquina, bool Estado);

public record TipoMaquinaDto(int Id, string Nombre, bool Estado);

public record PedidoFormOptionsDto(
    IEnumerable<ClienteDto> Clientes,
    IEnumerable<CatalogStringDto> FormasPago,
    IEnumerable<UsuarioDto> Vendedores,
    IEnumerable<CatalogStringDto> EstadosPago,
    IEnumerable<EstadoVentaOptionDto> EstadosVenta,
    IEnumerable<ProductoDto> Productos,
    IEnumerable<TipoMaquinaDto> Maquinas,
    int? UsuarioActualId,
    bool PuedeVerTodosPedidos);

public record ExcelFileDto(string FileName, string ContentType, string Bytes);

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

public record VentaImpresionCompletaUpdateRequest(
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
    IEnumerable<VentaImpresionDetalleUpdateRequest> Detalles);

public record EliminarPedidoRequest(string Observacion);

public record VentaImpresionDetalleUpdateRequest(
    int? Id,
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
    string? Vendedor,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    string? EstadoPagado,
    DateTime? FechaCreacion,
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
