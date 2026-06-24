using MiniERP.Application.DTOs.Units;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Services
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _unitRepository;

        public UnitService(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<IEnumerable<UnitResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var units = await _unitRepository.GetAllAsync(cancellationToken);
            return units.Select(u => new UnitResponse
            {
                Id = u.Id,
                UnitName = u.UnitName,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<UnitResponse> CreateAsync(CreateUnitRequest request, CancellationToken cancellationToken = default)
        {
            var unit = new Unit
            {
                UnitName = request.UnitName
            };

            _unitRepository.Add(unit);
            await _unitRepository.SaveChangesAsync(cancellationToken);

            return new UnitResponse
            {
                Id = unit.Id,
                UnitName = unit.UnitName,
                CreatedAt = unit.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateUnitRequest request, CancellationToken cancellationToken = default)
        {
            var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit == null) return false;

            unit.UnitName = request.UnitName;

            _unitRepository.Update(unit);
            await _unitRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit == null) return false;

            // Guard: Cannot delete if unit has products
            var hasProducts = await _unitRepository.HasProductsAsync(id, cancellationToken);
            if (hasProducts)
            {
                throw new InvalidOperationException("Cannot delete unit because it has associated products.");
            }

            _unitRepository.Delete(unit);
            await _unitRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
