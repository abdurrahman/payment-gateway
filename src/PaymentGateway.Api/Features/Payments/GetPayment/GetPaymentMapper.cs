#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

using PaymentGateway.Api.Infrastructure.Persistence;

public static class GetPaymentMapper
{
    public static GetPaymentResponse ToResponse(PaymentRecord payment)
    {
        return new GetPaymentResponse
        {
            Id = payment.Id,
            LastFourCardDigits = payment.LastFourCardDigits,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount,
            Status = payment.Status,
        };
    }
}
