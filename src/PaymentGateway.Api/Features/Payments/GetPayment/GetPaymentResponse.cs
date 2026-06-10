#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

/// <summary>
/// Represents a payment returned by the retrieval endpoint.
/// </summary>
public class GetPaymentResponse
{
    /// <summary>
    /// Unique payment identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Last four digits of the card number.
    /// </summary>
    public string? LastFourCardDigits { get; set; }

    /// <summary>
    /// Card expiry month.
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year.
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Three-letter ISO currency code.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Amount in minor units.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Final payment status.
    /// </summary>
    public PaymentStatus Status { get; set; }
}
