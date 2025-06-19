using OrdersService.Models;
using System.Text.Json;

namespace OrdersService.Services;

public class PaymentServiceClient : IPaymentServiceClient
{

    private readonly HttpClient _httpClient;

    private readonly ILogger<PaymentServiceClient> _logger;

    public PaymentServiceClient(HttpClient httpClient, ILogger<PaymentServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AccountResponse?> GetAccountByID(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:8001/api/account/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Account with id: {id} not found");
                    return null;
                }
                _logger.LogError("Failure during account fetching");
                return null;
            }
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<AccountResponse>(result, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching account");
            throw;
        }
    }
}