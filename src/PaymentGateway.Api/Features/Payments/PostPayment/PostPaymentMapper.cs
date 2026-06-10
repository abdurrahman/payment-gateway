#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

using PaymentGateway.Api.Infrastructure.BankClient;
using PaymentGateway.Api.Infrastructure.Persistence;

public static class PostPaymentMapper
{
    public static PaymentRecord ToRecord(
        Guid id,
        PostPaymentRequest request,
        PaymentStatus status,
        string idempotencyKey,
        string? authorizationCode)
    {
        return new PaymentRecord
        {
            Id = id,
            IdempotencyKey = idempotencyKey,
            AuthorizationCode = authorizationCode,
            LastFourCardDigits = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Status = status,
        };
    }

    public static PostPaymentResponse ToResponse(PaymentRecord payment)
    {
        return new PostPaymentResponse
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

    public static BankPaymentRequest ToBankRequest(PostPaymentRequest request)
    {
        return new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };
    }
}