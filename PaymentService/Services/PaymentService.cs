using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class PaymentsService : IPaymentService
    {
        private readonly PaymentDbContext _context;
        private readonly ILogger<PaymentsService> _logger;

        public PaymentsService(PaymentDbContext context, ILogger<PaymentsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AccountResponse> CreateAccountAsync()
        {
            try
            {
                var account = new Account
                {
                    UserId = Guid.NewGuid(),
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Account created successfully for UserId: {UserId}", account.UserId);

                return new AccountResponse
                {
                    UserId = account.UserId,
                    Balance = account.Balance,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                throw;
            }
        }

        public async Task<AccountResponse?> GetAccountAsync(Guid userId)
        {
            try
            {
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (account == null)
                {
                    _logger.LogWarning("Account not found for UserId: {UserId}", userId);
                    return null;
                }

                return new AccountResponse
                {
                    UserId = account.UserId,
                    Balance = account.Balance,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account for UserId: {UserId}", userId);
                throw;
            }
        }

public async Task<AccountResponse?> DepositAsync(DepositRequest request)
{
    var strategy = _context.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == request.UserId);

            if (account == null)
            {
                _logger.LogWarning("Account not found for deposit. UserId: {UserId}", request.UserId);
                return null;
            }

            account.Balance += request.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Deposit successful. UserId: {UserId}, Amount: {Amount}, New Balance: {Balance}",
                request.UserId, request.Amount, account.Balance);

            return new AccountResponse
            {
                UserId = account.UserId,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing deposit for UserId: {UserId}", request.UserId);
            throw;
        }
    });
}


        public async Task<BalanceResponse?> GetBalanceAsync(Guid userId)
        {
            try
            {
                var account = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (account == null)
                {
                    _logger.LogWarning("Account not found for balance check. UserId: {UserId}", userId);
                    return null;
                }

                return new BalanceResponse
                {
                    UserId = account.UserId,
                    Balance = account.Balance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance for UserId: {UserId}", userId);
                throw;
            }
        }
    }
}
