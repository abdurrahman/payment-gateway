using PaymentGateway.Api.Features.Payments;

namespace PaymentGateway.Api.Tests;

public class PaymentRequestValidatorTests
{
    [Fact]
    public void Validate_WithValidRequest_ShouldReturnValid()
    {
        var result = PaymentRequestValidator.Validate(BuildValid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")] // too short (< 14)
    [InlineData("12345678901234567890")] // too long (> 19)
    [InlineData("411111111111111A")] // non-numeric
    public void Validate_WithInvalidCardNumber_ShouldReturnError(string cardNumber)
    {
        var result = PaymentRequestValidator.Validate(BuildValid() with { CardNumber = cardNumber });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("card number", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_WithInvalidExpiryMonth_ShouldReturnError(int month)
    {
        var result = PaymentRequestValidator.Validate(BuildValid() with { ExpiryMonth = month });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("month", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithExpiredCard_ShouldReturnError()
    {
        var request = BuildValid() with { ExpiryYear = 2020, ExpiryMonth = 1 };

        var result = PaymentRequestValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("expired", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("XYZ")]  // unsupported
    [InlineData("GB")]   // too short
    [InlineData("GBPP")] // too long
    [InlineData("")]     // missing
    public void Validate_WithInvalidCurrency_ShouldReturnError(string currency)
    {
        var result = PaymentRequestValidator.Validate(BuildValid() with { Currency = currency });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("currency", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithZeroAmount_ShouldReturnError()
    {
        var result = PaymentRequestValidator.Validate(BuildValid() with { Amount = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("amount", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("12")]    // too short
    [InlineData("12345")] // too long
    [InlineData("12A")]   // non-numeric
    public void Validate_WithInvalidCvv_ShouldReturnError(string cvv)
    {
        var result = PaymentRequestValidator.Validate(BuildValid() with { Cvv = cvv });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("cvv", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        var request = BuildValid() with { CardNumber = "", Currency = "XYZ", Amount = 0 };

        var result = PaymentRequestValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
    }

    private static PostPaymentRequest BuildValid() => new()
    {
        CardNumber = "4111111111111111",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = 1000,
        Cvv = "123"
    };
}
