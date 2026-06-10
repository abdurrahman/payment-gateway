namespace PaymentGateway.Api.Infrastructure.BankClient;

using System.Text.Json.Serialization;

public record BankPaymentResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; init; }

    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; init; }
}