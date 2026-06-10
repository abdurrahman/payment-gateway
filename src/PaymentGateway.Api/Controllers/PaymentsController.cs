namespace PaymentGateway.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Features.Payments;

/// <summary>
/// Handles payment creation and retrieval operations.
/// </summary>
[Route("api/v1/payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly GetPaymentHandler _getPaymentHandler;
    private readonly PostPaymentHandler _postPaymentHandler;

    public PaymentsController(GetPaymentHandler getPaymentHandler, PostPaymentHandler postPaymentHandler)
    {
        _getPaymentHandler = getPaymentHandler;
        _postPaymentHandler = postPaymentHandler;
    }

    /// <summary>
    /// Retrieves a payment by its identifier.
    /// </summary>
    /// <param name="id">Unique payment identifier.</param>
    /// <returns>Payment details when found; otherwise 404.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPaymentResponse), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GetPaymentResponse?> GetPayment(Guid id)
    {
        var payment = _getPaymentHandler.Handle(id);
        if  (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Submits a payment for processing.
    /// </summary>
    /// <param name="request">Payment request payload.</param>
    /// <param name="idempotencyKey">Client-provided key used to prevent duplicate processing on retries.</param>
    /// <param name="cancellationToken">Cancellation token propagated from the HTTP request.</param>
    /// <returns>
    /// 201 when a new payment is created, 200 when the idempotency key already exists,
    /// 400 for missing required header, 422 for validation failures, or 502 when bank processing fails.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(
        [FromBody] PostPaymentRequest request,
        [FromHeader(Name = "Cko-Idempotency-Key")][Required] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await _postPaymentHandler.HandleAsync(request, idempotencyKey, cancellationToken);

        return result.StatusCode switch
        {
            StatusCodes.Status200OK => Ok(result.Response),
            StatusCodes.Status201Created => CreatedAtAction(nameof(GetPayment), new { id = result.Response!.Id }, result.Response),
            StatusCodes.Status400BadRequest => BadRequest(result.ErrorMessage),
            StatusCodes.Status422UnprocessableEntity => UnprocessableEntity(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status502BadGateway, result.ErrorMessage)
        };
    }
}
