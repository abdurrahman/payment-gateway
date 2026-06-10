#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

using PaymentGateway.Api.Infrastructure.BankClient;
using PaymentGateway.Api.Infrastructure.Persistence;
using Polly.CircuitBreaker;

public class PostPaymentHandler
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IBankClient _bankClient;
    private readonly ILogger<PostPaymentHandler> _logger;

    public PostPaymentHandler(PaymentsRepository paymentsRepository, IBankClient bankClient, ILogger<PostPaymentHandler> logger)
    {
        _paymentsRepository = paymentsRepository;
        _bankClient = bankClient;
        _logger = logger;
    }

    public async Task<PostPaymentResult> HandleAsync(PostPaymentRequest request, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting payment processing for idempotency key {IdempotencyKey}", idempotencyKey);

        if (string.IsNullOrEmpty(idempotencyKey))
        {
            return PostPaymentResult.BadRequest("Cko-Idempotency-Key header is required");
        }

        var existing = _paymentsRepository.GetByIdempotencyKey(idempotencyKey);
        if (existing is not null)
        {
            _logger.LogInformation(
                "Idempotent request detected for key {IdempotencyKey}, returning existing payment {PaymentId}",
                idempotencyKey,
                existing.Id);
            return PostPaymentResult.Ok(PostPaymentMapper.ToResponse(existing));
        }

        var validationResult = PaymentRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "Payment request validation failed for idempotency key {IdempotencyKey}: {Errors}",
                idempotencyKey,
                validationResult.Errors);
            return PostPaymentResult.Unprocessable(validationResult.Errors);
        }

        BankPaymentResponse? bankResponse;
        try
        {
            bankResponse = await _bankClient.ProcessPaymentAsync(PostPaymentMapper.ToBankRequest(request), cancellationToken);
        }
        catch (BankBadRequestException ex)
        {
            _logger.LogError(ex,
                "Bank rejected mapped request with 400 for idempotency key {IdempotencyKey}",
                idempotencyKey);
            return PostPaymentResult.BadGateway("Bank rejected the request payload");
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker open - bank unavailable for idempotency key {IdempotencyKey}", idempotencyKey);
            return PostPaymentResult.BadGateway("Bank temporarily unavailable");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Bank HTTP request failed for idempotency key {IdempotencyKey}. UpstreamStatusCode: {UpstreamStatusCode}",
                idempotencyKey,
                ex.StatusCode);
            return PostPaymentResult.BadGateway("Bank unavailable");
        }

        if (bankResponse is null)
        {
            _logger.LogError("Bank returned a null response for idempotency key {IdempotencyKey}", idempotencyKey);
            return PostPaymentResult.BadGateway("Invalid response from bank");
        }

        var status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
        var id = Guid.NewGuid();
        var record = PostPaymentMapper.ToRecord(id, request, status, idempotencyKey, bankResponse.AuthorizationCode);

        _paymentsRepository.Add(record);

        _logger.LogInformation(
            "Payment {PaymentId} processed with status {PaymentStatus} for idempotency key {IdempotencyKey}",
            record.Id,
            status,
            idempotencyKey);
        return PostPaymentResult.Created(PostPaymentMapper.ToResponse(record));
    }
}
