using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Primitives;
using PaymentGateway.Api.Features.Payments;
using PaymentGateway.Api.Infrastructure.BankClient;
using PaymentGateway.Api.Infrastructure.Persistence;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddScoped<GetPaymentHandler>();
builder.Services.AddScoped<PostPaymentHandler>();

var bankSimulatorBaseUrl = builder.Configuration["BankSimulator:BaseUrl"]
    ?? throw new InvalidOperationException("BankSimulator:BaseUrl is not configured");

builder.Services.AddHttpClient<IBankClient, BankClient>(client =>
{
    client.BaseAddress = new Uri(bankSimulatorBaseUrl);
}).AddResilienceHandler("bank-pipeline", pipeline =>
{
    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
    });

    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(30),
        FailureRatio = 0.5,
        MinimumThroughput = 5,
        BreakDuration = TimeSpan.FromSeconds(15),
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();  // TLS termination belongs at the ingress/load balancer level, not inside the container.

app.Use(async (context, next) =>
{
    const string requestIdHeader = "X-Request-Id";
    var requestId = context.Request.Headers.TryGetValue(requestIdHeader, out StringValues providedRequestId)
        && !StringValues.IsNullOrEmpty(providedRequestId)
            ? providedRequestId.ToString()
            : Guid.NewGuid().ToString();

    context.TraceIdentifier = requestId;
    context.Response.Headers[requestIdHeader] = requestId;

    using (app.Logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId }))
    {
        await next();
    }
});

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program { }