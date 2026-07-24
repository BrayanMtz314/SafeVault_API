using Microsoft.EntityFrameworkCore;
using SafeVault.Api.Data;
using SafeVault.Api.DTOs;
using SafeVault.Api.Models;

namespace SafeVault.Api.Services;

public class FinancialRecordService : IFinancialRecordService
{
    private readonly ApplicationDbContext _context;

    public FinancialRecordService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FinancialRecordResponseDto>> GetAllForUserAsync(string userId, bool isAdmin)
    {
        // SECURITY: If not admin, restrict query strictly to the authenticated user's records (Prevents IDOR)
        var query = _context.FinancialRecords.AsQueryable();
        
        if (!isAdmin)
        {
            query = query.Where(r => r.UserId == userId);
        }

        var records = await query.ToListAsync();
        return records.Select(MapToResponseDto);
    }

    public async Task<FinancialRecordResponseDto?> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        // LINQ automatically parameterizes 'id', preventing SQL injection
        var record = await _context.FinancialRecords.FirstOrDefaultAsync(r => r.Id == id);
        
        if (record == null) return null;

        // Security check: ensure regular users cannot fetch someone else's record by guessing the ID
        if (!isAdmin && record.UserId != userId) return null;

        return MapToResponseDto(record);
    }

    public async Task<FinancialRecordResponseDto> CreateAsync(CreateFinancialRecordDto dto, string userId)
    {
        var record = new FinancialRecord
        {
            UserId = userId,
            AccountName = dto.AccountName,
            Balance = dto.Balance,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FinancialRecords.Add(record);
        await _context.SaveChangesAsync();

        return MapToResponseDto(record);
    }

    public async Task<FinancialRecordResponseDto?> UpdateAsync(int id, UpdateFinancialRecordDto dto, string userId, bool isAdmin)
    {
        var record = await _context.FinancialRecords.FirstOrDefaultAsync(r => r.Id == id);
        
        if (record == null) return null;
        if (!isAdmin && record.UserId != userId) return null;

        record.AccountName = dto.AccountName;
        record.Balance = dto.Balance;
        record.Description = dto.Description;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponseDto(record);
    }

    public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
    {
        var record = await _context.FinancialRecords.FirstOrDefaultAsync(r => r.Id == id);
        
        if (record == null) return false;
        if (!isAdmin && record.UserId != userId) return false;

        _context.FinancialRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    private static FinancialRecordResponseDto MapToResponseDto(FinancialRecord record)
    {
        return new FinancialRecordResponseDto
        {
            Id = record.Id,
            UserId = record.UserId,
            AccountName = record.AccountName,
            Balance = record.Balance,
            Description = record.Description,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }
}