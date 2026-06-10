namespace PaymentGateway.Api.Infrastructure.Persistence;

using PaymentGateway.Api.Features.Payments;

public class PaymentRecord
{
    public Guid Id { get; set;}

    /// <summary>
    /// A unique key provided by the client to ensure idempotency of payment requests. 
    /// This key is used to prevent duplicate payments in case of retries or network issues. 
    /// </summary>
    public required string IdempotencyKey { get; set; }

    /// <summary>
    /// An optional code returned by the bank to indicate the result of the payment authorization.
    /// </summary>
    public string? AuthorizationCode {get; set;}

    /// <summary>
    /// The last 4 digits of the card used for the payment.
    /// </summary>
    public string? LastFourCardDigits { get; set; }

    /// <summary>
    /// The month component of the card's expiration date. 
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// The year component of the card's expiration date.
    /// </summary>
    public int ExpiryYear { get; set; }
    
    /// <summary>
    /// The three-letter ISO 4217 currency code of the payment.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// The amount of the payment.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// The status of the payment.
    /// </summary>
    public PaymentStatus Status { get; set; }
}
