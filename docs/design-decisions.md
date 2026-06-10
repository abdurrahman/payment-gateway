# Design Decisions

### 1. Vertical Slice Architecture
Code is organised by feature (Features/Payments/PostPayment/, Features/Payments/GetPayment/) rather than by layer (controllers/services/repositories). Each slice owns its own handler, mapper, request, response, and result types.

Feature organization will follow `Features/<Domain>/<Verb-Noun-Feature-Name>` convention.

### 2. Namespace strategy to avoid namespace pollution

Although files are split into subfolders (`GetPayment`, `PostPayment`, `Shared`), they intentionally use a single feature namespace:

`PaymentGateway.Api.Features.Payments`

Rationale:
- Keep feature types discoverable from one namespace.
- Avoid proliferating many narrow namespaces for closely related slice types.
- Keep consuming code (`Controllers`, tests, and handlers) simpler.

The folder and namespace mismatch is intentional and documented with targeted pragma(IDE0130) suppression in slice files.

### 3. Handler Pattern (thin controller)
The controller in PaymentsController.cs contains zero business logic — it only delegates to GetPaymentHandler and PostPaymentHandler and translates PostPaymentResult.StatusCode to an HTTP response. All logic lives in the handlers.

### 4. Result Object instead of Exceptions for flow control
PostPaymentResult (PostPaymentResult.cs) is a discriminated-union-style record with static factory methods (Created, Ok, BadGateway, etc.). The handler never throws for expected outcomes — it returns a typed result.

### 5. Idempotency via client-supplied key
The Cko-Idempotency-Key header is required on POST. The handler short-circuits with a 200 if that key already exists in the repository, preventing duplicate charges on retries. Stored on the PaymentRecord.

### 6. Card data minimisation
The full card number is never stored. Only LastFourCardDigits is written to PaymentRecord at the point of mapping. CVV is also never persisted.

### 7. In-memory persistence with thread-safe store
PaymentsRepository uses ConcurrentDictionary and is registered as a Singleton. Intentional for this stage — no database dependency, data is lost on restart.

### 8. Bank client resilience (retry + circuit breaker)
BankClient is wrapped with a Polly pipeline. 3 retries with exponential backoff, then a circuit breaker that trips at 50% failure rate over 30 seconds. Keeps the bank downstream from cascading failures into the gateway.

### 9. Infrastructure abstraction via IBankClient
The bank HTTP client is hidden behind IBankClient, making it swappable in tests without touching any feature code. Tests use `NSubstitute` to substitute the bank client.

### 10. Request tracing middleware
Request identification is handled via `X-Request-Id` (client supplied or server generated), echoed in the response and used in log scope for traceability, and intentionally not persisted in storage.

### 11. Environment and versioning boundaries
Gateway endpoints stay versioned under `/api/v1`, while the bank base URL is environment-specific via configuration (`BankSimulator:BaseUrl`) so sandbox/production hosts can be swapped without API contract changes.

### 12. No HTTPS in container
UseHttpsRedirection is commented out with explicit reasoning: TLS termination is the responsibility of the ingress/load balancer, not the app container. This is conventional for containerised APIs behind a reverse proxy.

### 13. Enum serialized as string
JsonStringEnumConverter is added globally so PaymentStatus comes back as "Authorized" / "Declined" / "Rejected" in JSON rather than integers.
