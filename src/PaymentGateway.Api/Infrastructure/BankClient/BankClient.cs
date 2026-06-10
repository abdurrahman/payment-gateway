namespace PaymentGateway.Api.Infrastructure.BankClient;

using System.Net;

public class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankClient> _logger;

    public BankClient(HttpClient httpClient, ILogger<BankClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BankPaymentResponse?> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/payments", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Bank rejected request with 400 BadRequest. Body: {BankErrorBody}", errorBody);
            throw new BankBadRequestException(errorBody);
        }

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new HttpRequestException("Bank returned ServiceUnavailable", null, response.StatusCode);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Bank returned unexpected status code {(int)response.StatusCode}", null, response.StatusCode);
        }

        var bankResponse = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken);
        if (bankResponse is null)
        {
            throw new HttpRequestException("Bank returned empty response body", null, response.StatusCode);
        }

        return bankResponse;
    }
}
