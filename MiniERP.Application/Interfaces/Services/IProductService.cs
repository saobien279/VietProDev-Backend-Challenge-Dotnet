using MiniERP.Application.DTOs.Products;
using MiniERP.Application.DTOs.Common;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryDto query, CancellationToken cancellationToken = default);
        Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
