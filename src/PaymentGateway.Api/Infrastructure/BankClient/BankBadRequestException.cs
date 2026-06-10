namespace PaymentGateway.Api.Infrastructure.BankClient;

public sealed class BankBadRequestException : Exception
{
    public BankBadRequestException(string message)
        : base(message)
    {
    }
}
