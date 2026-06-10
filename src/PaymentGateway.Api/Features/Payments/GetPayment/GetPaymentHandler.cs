#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace PaymentGateway.Api.Features.Payments;
#pragma warning restore IDE0130

using PaymentGateway.Api.Infrastructure.Persistence;

public class GetPaymentHandler
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly ILogger<GetPaymentHandler> _logger;

    public GetPaymentHandler(PaymentsRepository paymentsRepository, ILogger<GetPaymentHandler> logger)
    {
        _paymentsRepository = paymentsRepository;
        _logger = logger;
    }

    public GetPaymentResponse? Handle(Guid id)
    {
        _logger.LogDebug("Starting payment lookup for payment id {PaymentId}", id);

        var payment = _paymentsRepository.Get(id);
        if (payment is null)
        {
            _logger.LogDebug("Payment lookup completed for payment id {PaymentId} with no match", id);
            return null;
        }

        _logger.LogDebug("Payment lookup completed for payment id {PaymentId} with a match", id);
        return GetPaymentMapper.ToResponse(payment);
    }
}
