
using OrdersService.Models;

namespace OrdersService.Services
{
    public interface IOrdersService
    {
        Task<OrderResponse?> CreateOrder(CreateOrderRequest request);
        Task<IEnumerable<OrderResponse>?> GetAllOrders(Guid accountId);
        Task<StatusResponse?> GetOrderStatus(Guid id);
    }
}
