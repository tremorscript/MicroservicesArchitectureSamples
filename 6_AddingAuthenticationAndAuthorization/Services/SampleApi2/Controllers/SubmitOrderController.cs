using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sample.Contracts;
using SampleApi2.Models;

namespace SampleApi2.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SubmitOrderController : ControllerBase
{
    private ILogger<SubmitOrderController> _logger;
    private IRequestClient<SubmitOrder> _submitOrderRequestClient;
    private ISendEndpointProvider _sendEndpointProvider;
    private IPublishEndpoint _publishEndpoint;

    public SubmitOrderController(ILogger<SubmitOrderController> logger, IRequestClient<SubmitOrder> submitOrderRequestClient,
    ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _submitOrderRequestClient = submitOrderRequestClient;
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> Post(OrderViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(new
        {
            OrderId = model.Id,
            InVar.Timestamp,
            model.CustomerNumber,
            model.Notes
        });

        if (accepted.IsCompletedSuccessfully)
        {
            var response = await accepted;

            return Accepted(response);
        }

        if (accepted.IsCompleted)
        {
            await accepted;

            return Problem("Order was not accepted");
        }
        else
        {
            var response = await rejected;

            return BadRequest(response.Message);
        }
    }
}