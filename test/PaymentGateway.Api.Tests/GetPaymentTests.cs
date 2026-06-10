using System.Net;
using System.Net.Http.Json;

using PaymentGateway.Api.Features.Payments;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Tests;

public class GetPaymentTests : PaymentIntegrationTestBase
{
    private readonly Random _random = new();

    [Fact]
    public async Task RetrievePayment_WithValidId_ShouldReturnPayment()
    {
        // Arrange        
        var paymentRecord = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            ExpiryYear = _random.Next(2027, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            LastFourCardDigits = "8877",
            Currency = "GBP",
            Status = PaymentStatus.Authorized
        };

        _paymentsRepository.Add(paymentRecord);

        // Act
        var response = await _client.GetAsync($"/api/v1/payments/{paymentRecord.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>(_testJsonOptions);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentRecord.Id, paymentResponse!.Id);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse.Status);
        Assert.Equal("8877", paymentResponse.LastFourCardDigits);
        Assert.Equal(paymentRecord.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(paymentRecord.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(paymentRecord.Currency, paymentResponse.Currency);
        Assert.Equal(paymentRecord.Amount, paymentResponse.Amount);
    }

    [Fact]
    public async Task RetrievePayment_WithUnknownId_ShouldReturnNotFound()
    {       
        // Act
        var response = await _client.GetAsync($"/api/v1/payments/{Guid.NewGuid()}");
        
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RetrievePayment_WithoutRequestIdHeader_ShouldReturnGeneratedRequestIdHeader()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/payments/{Guid.NewGuid()}");

        // Assert
        Assert.True(response.Headers.TryGetValues("X-Request-Id", out var requestIdValues));
        var requestId = Assert.Single(requestIdValues);
        Assert.True(Guid.TryParse(requestId, out _));
    }

    [Fact]
    public async Task RetrievePayment_WithRequestIdHeader_ShouldEchoSameRequestIdHeader()
    {
        // Arrange
        var expectedRequestId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/payments/{Guid.NewGuid()}");
        request.Headers.Add("X-Request-Id", expectedRequestId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.TryGetValues("X-Request-Id", out var requestIdValues));
        Assert.Equal(expectedRequestId, Assert.Single(requestIdValues));
    }
}