#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

/// <summary>
/// Represents payment processing outcomes.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Bank authorized the payment.
    /// </summary>
    Authorized,

    /// <summary>
    /// Bank declined the payment.
    /// </summary>
    Declined,

    /// <summary>
    /// Payment was rejected before authorization.
    /// </summary>
    Rejected
}