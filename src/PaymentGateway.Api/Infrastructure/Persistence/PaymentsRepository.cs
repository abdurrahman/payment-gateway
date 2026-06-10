namespace PaymentGateway.Api.Infrastructure.Persistence;

using System.Collections.Concurrent;

public class PaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, PaymentRecord> _payments = new ();
    
    public void Add(PaymentRecord payment)
    {
        _payments.TryAdd(payment.Id, payment);
    }

    public PaymentRecord? Get(Guid id)
    {
        _payments.TryGetValue(id, out var payment);
        return payment;
    }

    public PaymentRecord? GetByIdempotencyKey(string key)
    {
        return _payments.Values.FirstOrDefault(payment => payment.IdempotencyKey == key);
    }
}
