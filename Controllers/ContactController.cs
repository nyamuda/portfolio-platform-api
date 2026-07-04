using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Contact;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Contact;

namespace PortfolioPlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactController(
    IContactService contactService,
    ILogger<ContactController> logger
) : ControllerBase
{
    private readonly IContactService _contactService = contactService;
    private readonly ILogger<ContactController> _logger = logger;

    /// <summary>
    /// Accepts a public contact message.
    /// </summary>
    /// <param name="dto">The contact message submitted by the visitor.</param>
    [HttpPost]
    public async Task<IActionResult> Post(ContactDto dto)
    {
        try
        {
            await _contactService.SendAsync(dto);
            return NoContent();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process a contact form message.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }
}
