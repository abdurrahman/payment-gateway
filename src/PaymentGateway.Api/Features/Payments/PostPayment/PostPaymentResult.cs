#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

public record PostPaymentResult
{
    public int StatusCode { get; init; }
    public PostPaymentResponse? Response { get; init; }
    public IReadOnlyCollection<string>? ValidationErrors { get; init; }
    public string? ErrorMessage { get; init; }

    public static PostPaymentResult Created(PostPaymentResponse response) =>
        new() { StatusCode = StatusCodes.Status201Created, Response = response };

    public static PostPaymentResult Ok(PostPaymentResponse response) =>
        new() { StatusCode = StatusCodes.Status200OK, Response = response };

    public static PostPaymentResult Unprocessable(IReadOnlyCollection<string> errors) =>
        new() { StatusCode = StatusCodes.Status422UnprocessableEntity, ValidationErrors = errors };

    public static PostPaymentResult BadRequest(string message) =>
        new() { StatusCode = StatusCodes.Status400BadRequest, ErrorMessage = message };

    public static PostPaymentResult BadGateway(string message) =>
        new() { StatusCode = StatusCodes.Status502BadGateway, ErrorMessage = message };
}
