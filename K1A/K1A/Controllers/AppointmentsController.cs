using K1A.Models.DTOs;

namespace K1A.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentsService _appointmentsService;

    public AppointmentsController(IAppointmentsService appointmentsService)
    {
        _appointmentsService = appointmentsService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentsService.GetAppointmentByIdAsync(id);

        if (appointment == null)
            return NotFound(new { message = "Appointment not found." });

        return Ok(appointment);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] AddAppointmentRequestDto request)
    {
        try
        {
            await _appointmentsService.AddAppointmentAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = request.AppointmentId }, null);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
