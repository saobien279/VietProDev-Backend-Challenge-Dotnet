using MiniERP.Application.DTOs.Units;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UnitResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UnitResponse> CreateAsync(CreateUnitRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id, UpdateUnitRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
