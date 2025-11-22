using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs.Commerce;
using ArtemisBanking.Application.DTOs.Common;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface ICommerceServiceApi
    {
        Task<Result<PagedResult<CommerceListDto>>> GetAllAsync(int? page, int? pageSize);
        Task<Result<CommerceDetailDto>> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(CommerceCreateUpdateDto dto);
        Task<Result> UpdateAsync(int id, CommerceCreateUpdateDto dto);
        Task<Result> ChangeStatusAsync(int id, bool status);
    }
}
