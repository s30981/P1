using K1B.Models.DTOs;
using K1B.Services;

namespace K1B.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IVisitsServices _visitService;

    public VisitsController(IVisitsServices visitService)
    {
        _visitService = visitService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var visit = await _visitService.GetVisitByIdAsync(id);
        if (visit == null)
            return NotFound(new { message = $"Visit with id {id} not found." });

        return Ok(visit);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VisitCreateDto visitDto)
    {
        try
        {
            await _visitService.AddVisitAsync(visitDto);
            return CreatedAtAction(nameof(GetById), new { id = visitDto.VisitId }, null);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
