
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace InsightFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class AiTestController : ControllerBase
{
    private readonly Kernel? _kernel;

    public AiTestController(Kernel? kernel)
    {
        _kernel = kernel;
    }

    public sealed class ChatTestRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] ChatTestRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                message = "Message cannot be empty."
            });
        }

        if (_kernel is null)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "AI chat service is currently unavailable."
                });
        }

        try
        {
            var response = await _kernel.InvokePromptAsync(
                request.Message,
                cancellationToken: cancellationToken);

            return Ok(new
            {
                reply = response.ToString()
            });
        }
        catch (Exception ex)
        {
            var errorDetails = new List<string>();

            for (var current = ex; current != null; current = current.InnerException)
            {
                errorDetails.Add(
                    $"{current.GetType().Name}: {current.Message}");
            }

            return StatusCode(502, new
            {
                message = "Gemini request failed.",
                details = errorDetails
            });
        }
    }
}
