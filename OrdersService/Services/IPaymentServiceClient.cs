using OrdersService.Models;

namespace OrdersService.Services;

interface IPaymentServiceClient
{
    Task<AccountResponse?> GetAccountByID(Guid id);
}