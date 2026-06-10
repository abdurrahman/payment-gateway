# Payment Gateway v1.0.0

Payment Gateway backend solution.

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
CONTRIBUTING.MD
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

## Table of contents

- Getting Started
- Build
- Database
- Testing
- Running Services in Docker
- Design Decisions
- Before Production
- Contributing
- Related Projects

### Getting Started

Before you start make sure that:
- you have .NET 8.0 SDK installed
- you have Docker installed

### Build

Code can be compiled by running the following command:

```bash
dotnet build
```
or with your favorite IDE.

### Database

No database dependency. Payments are stored in an in-memory `ConcurrentDictionary` (see `PaymentsRepository`). Data does not persist across restarts — intentional for this stage. See [docs/design-decisions.md](docs/design-decisions.md) §7 and the Before Production section for the path to durable storage.

### Testing
Unit tests are located in `tests/PaymentGateway.Api.Tests/`.

Test can be executed by running the following command:
```bash
dotnet test
```

POST a payment through the gateway:

```shell
curl -X POST http://localhost:8081/api/v1/payments \
  -H "Content-Type: application/json" \
  -H "Cko-Idempotency-Key: test-key-001" \
  -d '{
    "cardNumber": "2222405343248877",
    "expiryMonth": 4,
    "expiryYear": 2027,
    "currency": "GBP",
    "amount": 1000,
    "cvv": "123"
  }'
```

PowerShell commands:

Health check:
```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:8081/health"
```

GET payment by id:

```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:8081/api/v1/payments/<guid>"
```

POST payment:
```powershell
$headers = @{
  "Cko-Idempotency-Key" = "test-key-001"
}

$body = @{
  cardNumber  = "2222405343248877"
  expiryMonth = 4
  expiryYear  = 2027
  currency    = "GBP"
  amount      = 1000
  cvv         = "123"
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8081/api/v1/payments" `
  -Headers $headers `
  -ContentType "application/json" `
  -Body $body
```

## Running Services in Docker

We use docker compose to run services to simulate production environment.

Run it:
```bash
docker compose up --build -d
```
`--build` forces the gateway image to rebuild from the Dockerfile rather than using a cached layer. Important during development.
`-d` detached mode - background

Check both containers are running:
```bash
docker compose ps
```
Both should show `running` and bank_simulator should show `healthy`.

Hit the gateway directly:
```bash
curl -X GET http://localhost:8081/api/v1/payments/{some-guid}
```
Expect 404 — that's correct, no payments exist yet.

Hit the bank simulator directly:
```shell
curl http://localhost:2525
```
Mountebank admin UI should respond.

Check logs if something's wrong:
```shell
docker compose logs payment_gateway
docker compose logs bank_simulator
```

Tear down:
```shell
docker compose down --rmi local --remove-orphans --volumes
```

## Design Decisions

See [docs/design-decisions.md](docs/design-decisions.md) for the full list of architectural and implementation decisions.

## Before Production

The following items are knowingly deferred and would be required before any real deployment:

- **Luhn algorithm validation** — card numbers are not checksum-validated before hitting the bank; intentionally skipped here because the simulator uses last-digit behavior to control responses, not Luhn validity. A real gateway must validate before forwarding.
- **Authentication / authorisation** — no API key, JWT, or mTLS between merchant and gateway. `X-Request-Id` provides traceability only.
- **Idempotency key TTL** — keys are stored indefinitely in memory; in production they need an expiry window (e.g. 24 h) to prevent unbounded storage growth.
- **Persistent storage** — `ConcurrentDictionary` is in-process and data is lost on restart; a real deployment needs a durable store (e.g. PostgreSQL) with appropriate indexes on `IdempotencyKey`.
- **Card data handling / PCI scope** — full card number and CVV are currently passed in plaintext over HTTP internally; production requires end-to-end TLS, tokenisation, and PCI DSS controls.
- **Secrets management** — `BankSimulator:BaseUrl` and any future credentials must be supplied via a secrets manager (e.g. AWS Secrets Manager, Azure Key Vault), not plain configuration files.
- **Rate limiting** — no per-client throttling in place; a production gateway needs protection against abuse and runaway retry storms.
- **Structured log shipping** — logs go to stdout only; production needs a log aggregation pipeline (e.g. ELK, Datadog) with retention policy and alerting.
- **Metrics and alerting** — no counters or histograms for payment success/failure rates, latency, or circuit breaker state; these are essential for operational SLAs.
- **Contract/integration tests against the real simulator** — current tests mock the bank client; a separate integration test suite should run against the live Mountebank simulator to catch mapping and serialisation regressions end-to-end.

## Contributing

Refer to [the contributing guide](CONTRIBUTING.md) for detailed instructions on how to contribute to Payment Gateway.

## Related Projects

- mountebank - A cross-platform, multi-protocol test doubles
- ...