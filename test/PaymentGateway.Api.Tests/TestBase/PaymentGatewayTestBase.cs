using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using PaymentGateway.Api.Infrastructure.BankClient;
using PaymentGateway.Api.Infrastructure.Persistence;

public abstract class PaymentIntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly IBankClient _bankClientMock;
    protected readonly PaymentsRepository _paymentsRepository;
    protected readonly JsonSerializerOptions _testJsonOptions;

    protected PaymentIntegrationTestBase()
    {
        _bankClientMock = Substitute.For<IBankClient>();
        _paymentsRepository = new PaymentsRepository();

        _testJsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var repoDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PaymentsRepository));
                if (repoDescriptor != null) services.Remove(repoDescriptor);

                var clientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBankClient));
                if (clientDescriptor != null) services.Remove(clientDescriptor);

                services.AddSingleton(_paymentsRepository);
                services.AddSingleton(_bankClientMock);
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}