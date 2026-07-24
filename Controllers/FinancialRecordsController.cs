using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Api.DTOs;
using SafeVault.Api.Services;

namespace SafeVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// Note: We will fully test authentication in Part 3, but placing this here secures the whole controller!
[Authorize] 
public class FinancialRecordsController : ControllerBase
{
    private readonly IFinancialRecordService _service;

    public FinancialRecordsController(IFinancialRecordService service)
    {
        _service = service;
    }

    // Helper method to securely extract the logged-in user's ID from their JWT token claims
    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private bool IsCurrentUserAdmin() => User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinancialRecordResponseDto>>> GetAll()
    {
        var records = await _service.GetAllForUserAsync(GetCurrentUserId(), IsCurrentUserAdmin());
        return Ok(records);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FinancialRecordResponseDto>> GetById(int id)
    {
        var record = await _service.GetByIdAsync(id, GetCurrentUserId(), IsCurrentUserAdmin());
        if (record == null) return NotFound(new { message = "Record not found or access denied." });
        return Ok(record);
    }

    [HttpPost]
    public async Task<ActionResult<FinancialRecordResponseDto>> Create([FromBody] CreateFinancialRecordDto dto)
    {
        // [ApiController] automatically validates DTO annotations before reaching this line!
        var createdRecord = await _service.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = createdRecord.Id }, createdRecord);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FinancialRecordResponseDto>> Update(int id, [FromBody] UpdateFinancialRecordDto dto)
    {
        var updatedRecord = await _service.UpdateAsync(id, dto, GetCurrentUserId(), IsCurrentUserAdmin());
        if (updatedRecord == null) return NotFound(new { message = "Record not found or access denied." });
        return Ok(updatedRecord);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id, GetCurrentUserId(), IsCurrentUserAdmin());
        if (!success) return NotFound(new { message = "Record not found or access denied." });
        return NoContent();
    }
}