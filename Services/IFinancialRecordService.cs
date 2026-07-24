using SafeVault.Api.DTOs;

namespace SafeVault.Api.Services;

public interface IFinancialRecordService
{
    Task<IEnumerable<FinancialRecordResponseDto>> GetAllForUserAsync(string userId, bool isAdmin);
    Task<FinancialRecordResponseDto?> GetByIdAsync(int id, string userId, bool isAdmin);
    Task<FinancialRecordResponseDto> CreateAsync(CreateFinancialRecordDto dto, string userId);
    Task<FinancialRecordResponseDto?> UpdateAsync(int id, UpdateFinancialRecordDto dto, string userId, bool isAdmin);
    Task<bool> DeleteAsync(int id, string userId, bool isAdmin);
}