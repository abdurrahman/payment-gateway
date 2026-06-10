#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a payment creation request.
/// </summary>
public record PostPaymentRequest
{
    /// <summary>
    /// Full card number sent to the bank simulator (without separators).
    /// </summary>
    [Required]
    [JsonRequired]
    public required string CardNumber { get; init; }

    /// <summary>
    /// Card expiry month (1–12).
    /// </summary>
    [Required]
    [JsonRequired]
    public required int ExpiryMonth { get; init; }

    /// <summary>
    /// Four-digit card expiry year.
    /// </summary>
    [Required]
    [JsonRequired]
    public required int ExpiryYear { get; init; }

    /// <summary>
    /// Three-letter ISO currency code, for example GBP or USD.
    /// </summary>
    [Required]
    [JsonRequired]
    public required string Currency { get; init; }

    /// <summary>
    /// Amount in minor units, for example 1000 = 10.00.
    /// </summary>
    [Required]
    [JsonRequired]
    public required int Amount { get; init; }

    /// <summary>
    /// Card security code sent to the bank for verification.
    /// </summary>
    [Required]
    [JsonRequired]
    public required string Cvv { get; init; }
}