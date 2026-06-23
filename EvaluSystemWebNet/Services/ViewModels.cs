namespace EvaluSystemWebNet.Services;

public record PagedView<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record ClienteView(
    int Id,
    string Name,
    string Document,
    string Phone,
    string Email,
    string City,
    bool Carrier,
    string Status);

public record PedidoView(
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
    List<PedidoDetalleView> Details);

public record PedidoDetalleView(
    string Product,
    string Machine,
    string Quantity,
    string UnitPrice,
    string ExtraPrice,
    string DesignName);

public record DashboardView(
    DashboardMetrics Metrics,
    IEnumerable<DashboardOrderView> RecentOrders,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record DashboardMetrics(int LoadedToday, int Printed, int MissingPrint, string Delivered);

public record DashboardOrderView(
    string Id,
    string Client,
    string Seller,
    string Type,
    string Meters,
    string Status,
    string Delivery);
