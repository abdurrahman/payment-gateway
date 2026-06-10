using System.Net;
using System.Net.Http.Json;

using NSubstitute;

using PaymentGateway.Api.Features.Payments;
using PaymentGateway.Api.Infrastructure.BankClient;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Tests;

public class PostPaymentsTests : PaymentIntegrationTestBase
{

    [Fact]
    public async Task ProcessPayment_WithValidRequest_ShouldReturnAuthorized()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = CreateValidRequest();

        _bankClientMock
            .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<BankPaymentResponse?>(new BankPaymentResponse
            {
                Authorized = true,
                AuthorizationCode = "AUTH-OK-777"
            }));

        var httpRequest = CreatePostMessage(request, idempotencyKey);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(_testJsonOptions);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse!.Status);
        Assert.Equal("1111", paymentResponse.LastFourCardDigits);
        Assert.Equal(request.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(request.Currency, paymentResponse.Currency);
        Assert.Equal(request.Amount, paymentResponse.Amount);
    }

    [Fact]
    public async Task ProcessPayment_WithMissingIdempotencyKey_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreateValidRequest();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(request)
        };

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await _bankClientMock.DidNotReceiveWithAnyArgs().ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPayment_WhenBankIsUnavailable_ShouldReturnBadGateway()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = CreateValidRequest();

        _bankClientMock
            .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns<BankPaymentResponse?>(_ => throw new HttpRequestException("Bank unavailable", null, HttpStatusCode.ServiceUnavailable));

        var httpRequest = CreatePostMessage(request, idempotencyKey);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_WithExpiredCard_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 5,
            ExpiryYear = 2023, // expired
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var httpRequest = CreatePostMessage(request, idempotencyKey);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        
        // Verify the bank boundary was NEVER breached for an invalid request
        await _bankClientMock.DidNotReceiveWithAnyArgs().ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPayment_WithUnsupportedCurrency_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2028,
            Currency = "XYZ", // Non-existent/unsupported ISO currency code
            Amount = 1000,
            Cvv = "123"
        };

        var httpRequest = CreatePostMessage(request, idempotencyKey);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        
        await _bankClientMock.DidNotReceiveWithAnyArgs().ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPayment_WithDuplicateIdempotencyKey_ShouldReturnSamePayment()
    {
        // Arrange
        var existingIdempotencyKey = Guid.NewGuid().ToString();
        var existingRecordId = Guid.NewGuid();
        
        // 1. Seed an existing record directly into the repository to simulate a past successful request
        var historicalRecord = new PaymentRecord
        {
            Id = existingRecordId,
            LastFourCardDigits = "1111",
            ExpiryMonth = 12,
            ExpiryYear = 2028,
            Currency = "USD",
            Amount = 5000,
            Status = PaymentStatus.Authorized,
            IdempotencyKey = existingIdempotencyKey,
            AuthorizationCode = "AUTH-HISTORIC-111"
        };
        _paymentsRepository.Add(historicalRecord);

        // 2. Prepare an identical incoming request payload using the exact same key
        var duplicateRequest = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2028,
            Currency = "USD",
            Amount = 5000,
            Cvv = "123"
        };
        var httpRequest = CreatePostMessage(duplicateRequest, existingIdempotencyKey);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(_testJsonOptions);
        Assert.NotNull(paymentResponse);
        Assert.Equal(existingRecordId, paymentResponse!.Id); // Verifies the exact same resource identity is returned
        Assert.Equal("Authorized", paymentResponse.Status.ToString());

        await _bankClientMock.DidNotReceiveWithAnyArgs().ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPayment_WhenBankDeclines_ShouldReturnDeclinedStatus()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = CreateValidRequest();

        _bankClientMock
            .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<BankPaymentResponse?>(new BankPaymentResponse
            {
                Authorized = false,
                AuthorizationCode = null
            }));

        var httpRequest = CreatePostMessage(request, idempotencyKey);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(_testJsonOptions);
        Assert.Equal("Declined", paymentResponse?.Status.ToString());
    }

    private PostPaymentRequest CreateValidRequest() => new()
    {
        CardNumber = "4111111111111111",
        ExpiryMonth = 12,
        ExpiryYear = 2028,
        Currency = "USD",
        Amount = 5000,
        Cvv = "123"
    };

    private HttpRequestMessage CreatePostMessage(PostPaymentRequest request, string key)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(request)
        };
        msg.Headers.Add("Cko-Idempotency-Key", key);
        return msg;
    }
}
