#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

public static class PaymentRequestValidator
{
    private static readonly HashSet<string> SupportedCurrencies = new() { "USD", "EUR", "GBP" };

    public static PaymentValidationResult Validate(PostPaymentRequest request)
    {
         var result = new PaymentValidationResult();

        if (string.IsNullOrWhiteSpace(request.CardNumber))
            result.Errors.Add("Card number is required");
        else if (request.CardNumber.Length < 14 || request.CardNumber.Length > 19)
            result.Errors.Add("Card number must be between 14 and 19 digits");
        else if (!request.CardNumber.All(char.IsDigit))
            result.Errors.Add("Card number must contain only numeric characters");

        if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
        {
            result.Errors.Add("Expiry month must be between 1 and 12");
        }
        else if (request.ExpiryYear < DateTime.UtcNow.Year)
        {
            result.Errors.Add("Card has expired");
        }
        else
        {
            var expiry = new DateOnly(request.ExpiryYear, request.ExpiryMonth, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (expiry < today)
                result.Errors.Add("Card has expired");
        }

        if (string.IsNullOrEmpty(request.Currency))
            result.Errors.Add("Currency is required");
        else if (request.Currency.Length != 3)
            result.Errors.Add("Currency must be 3 characters");
        else if (!SupportedCurrencies.Contains(request.Currency.ToUpper()))
            result.Errors.Add("Currency must be one of: USD, EUR, GBP");

        if (request.Amount <= 0)
            result.Errors.Add("Amount must be greater than zero");

        if (string.IsNullOrEmpty(request.Cvv))
            result.Errors.Add("CVV is required");
        else if (request.Cvv.Length < 3 || request.Cvv.Length > 4)
            result.Errors.Add("CVV must be 3 or 4 characters");
        else if (!request.Cvv.All(char.IsDigit))
            result.Errors.Add("CVV must contain only numeric characters");

        return result;
    }
}

public class PaymentValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; } = new();
}
