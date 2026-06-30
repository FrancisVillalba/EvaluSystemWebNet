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
    string? Persona,
    IEnumerable<PerfilFormularioPermisoDto> Permisos);

public record ClienteDto(
    int Id,
    string? Nombre,
    string? Documento,
    string TipoDocumentoId,
    string TipoClienteId,
    string? Email,
    string? NroTelefono,
    string? Direccion,
    int? DepartamentoId,
    string? Departamento,
    int? CiudadId,
    string? Ciudad,
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
    int? DepartamentoId,
    int? CiudadId,
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

public record PerfilDto(int Id, string Nombre, string? Descripcion, bool Estado);

public record PersonaDto(
    int Id,
    int? PerfilId,
    string? Perfil,
    string? PrimerNombre,
    string? SegundoNombre,
    string? PrimerApellido,
    string? SegundoApellido,
    DateTime? FechaCumpleanios,
    string? TipoDocumentoId,
    string? Documento,
    bool? Estado);

public record FormularioDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? Ruta,
    string? Icono,
    int Orden,
    bool Estado);

public record PerfilFormularioPermisoDto(
    int Id,
    int PerfilId,
    string? Perfil,
    int FormularioId,
    string Formulario,
    string? Descripcion,
    string? Ruta,
    string? Icono,
    int Orden,
    bool PuedeVer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeEliminar);

public record PerfilFormularioPermisoRequest(
    int PerfilId,
    int FormularioId,
    bool PuedeVer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeEliminar);

public record AdminOptionsDto(
    IEnumerable<PerfilDto> Perfiles,
    IEnumerable<PersonaDto> Personas,
    IEnumerable<TipoMaquinaDto> Maquinas,
    IEnumerable<DepartamentoDto> Departamentos,
    IEnumerable<CatalogStringDto> TiposDocumento);

public record EstadoVentaOptionDto(string Id, string? Nombre, string? Estado, int? NumeroFlujo);

public record UsuarioDto(
    int Id,
    string? NombreUsuario,
    int? PersonaId,
    string? Persona,
    int? PerfilId,
    string? Perfil,
    IEnumerable<int> PerfilIds,
    string? Perfiles,
    bool? Estado);

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

public record ArchivoUploadResponse(
    bool Guardado,
    string NombreOriginal,
    string NombreDescarga,
    string NombreGuardado,
    string Ruta,
    long TamanioBytes);

public record ArchivoBase64Response(
    string NombreArchivo,
    string ContentType,
    string Base64,
    long TamanioBytes);

public record DeliveryPedidoDto(
    int Id,
    DateTime FechaCreacion,
    DateTime? FechaEntrega,
    string Cliente,
    string? Telefono,
    string? Direccion,
    string? Departamento,
    string? Ciudad,
    string EstadoVentaId,
    string? EstadoVenta,
    string Vendedor,
    decimal TotalVenta,
    string MetodoEntregaId,
    string MetodoEntrega,
    int? DeliveryUsuarioId,
    string? DeliveryUsuario,
    DateTime? FechaTomaDelivery,
    string Productos);

public record DeliveryResumenDto(
    int UsuarioId,
    string Delivery,
    int CantidadPedidos,
    decimal TotalPedidos,
    string Ciudades,
    string MetodosEnvio);

public record ReporteComisionesDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    IEnumerable<ReporteComisionVendedorDto> Vendedores);

public record ReporteComisionVendedorDto(
    int VendedorId,
    string Vendedor,
    int CantidadPedidos,
    decimal TotalVenta,
    decimal TotalComision,
    IEnumerable<ReporteComisionDetalleDto> Detalles);

public record ReporteComisionDetalleDto(
    int PedidoId,
    DateTime Fecha,
    string Cliente,
    string Producto,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal PrecioExtra,
    decimal TotalDetalle,
    decimal ComisionUnitario,
    decimal ComisionTotal);

public record LotePagoDto(
    int Id,
    string TipoPago,
    DateTime FechaGeneracion,
    string UsuarioGenero,
    DateTime FechaDesde,
    DateTime FechaHasta,
    DateTime FechaPago,
    string? Vendedor,
    decimal MontoTotal,
    int CantidadPersonas,
    string NombreArchivo,
    string Estado);

public record ReporteEnviosDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    IEnumerable<ReporteEnvioResumenDto> Resumen,
    IEnumerable<ReporteEnvioDetalleDto> Detalles);

public record ReporteEnvioResumenDto(
    int? UsuarioEntregaId,
    string UsuarioEntrega,
    int CantidadPedidos,
    int CantidadTransportadora,
    decimal TotalPedidos);

public record ReporteEnvioDetalleDto(
    int PedidoId,
    DateTime Fecha,
    string Cliente,
    string MetodoEntregaId,
    string MetodoEntrega,
    string Estado,
    string? UsuarioEntrega,
    string? Ciudad,
    decimal TotalPedido);

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
    string? MetodoEntrega,
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
    string? MetodoEntrega,
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
    string? MetodoEntrega,
    int? DeliveryUsuarioId,
    string? DeliveryUsuario,
    DateTime? FechaTomaDelivery,
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

public record ImpresionArchivoDto(
    int DetalleId,
    int PedidoId,
    DateTime FechaCarga,
    DateTime? FechaEntrega,
    string Cliente,
    int TipoMaquinaId,
    string TipoMaquina,
    string Producto,
    decimal Cantidad,
    string? ArchivoDisenioNombre,
    string EstadoVenta,
    bool Impreso);

public record ImpresionMarcarDto(
    int DetalleId,
    int PedidoId,
    bool DetalleImpreso,
    bool PedidoCompleto,
    string EstadoVentaId,
    string? EstadoVenta);
