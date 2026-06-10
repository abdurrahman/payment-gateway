namespace PaymentGateway.Api.Infrastructure.BankClient;

public interface IBankClient
{
    /// <summary>
    /// Sends a payment request to the bank and returns the response. 
    /// </summary>
    /// <param name="request">The payment request to be sent to the bank.</param>
    /// <param name="cancellationToken">Cancellation token for the outbound HTTP request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the bank's payment response.</returns>
    Task<BankPaymentResponse?> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}