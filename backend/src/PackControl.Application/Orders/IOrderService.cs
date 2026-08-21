namespace PackControl.Application.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderListItemDto>> ListAsync(CancellationToken cancellationToken);
    Task<OrderDetailDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDetailDto?> AttachFileAsync(
        Guid orderId,
        Stream fileStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken);
    Task<OrderDetailDto?> ApproveAsync(Guid orderId, CancellationToken cancellationToken);
}
