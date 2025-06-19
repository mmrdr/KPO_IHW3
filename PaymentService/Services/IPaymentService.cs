using PaymentService.Models;

namespace PaymentService.Services
{
    public interface IPaymentService
    {
        Task<AccountResponse> CreateAccountAsync();
        Task<AccountResponse?> GetAccountAsync(Guid userId);
        Task<AccountResponse?> DepositAsync(DepositRequest request);
        Task<BalanceResponse?> GetBalanceAsync(Guid userId);
    }
}
