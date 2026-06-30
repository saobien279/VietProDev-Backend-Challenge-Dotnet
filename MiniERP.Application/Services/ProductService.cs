using MiniERP.Application.DTOs.Products;
using MiniERP.Application.Exceptions;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductPriceHistoryRepository _priceHistoryRepository;

        public ProductService(
            IProductRepository productRepository,
            IProductPriceHistoryRepository priceHistoryRepository)
        {
            _productRepository = productRepository;
            _priceHistoryRepository = priceHistoryRepository;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);
            return products.Select(MapToResponse);
        }

        public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) return null;

            return MapToResponse(product);
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = new Product
            {
                Sku = request.Sku,
                ProductName = request.ProductName,
                CostPrice = request.CostPrice,
                SellingPrice = request.SellingPrice,
                CategoryId = request.CategoryId,
                UnitId = request.UnitId,
                IsActive = request.IsActive
            };

            _productRepository.Add(product);
            await _productRepository.SaveChangesAsync(cancellationToken);

            // Re-fetch to include Category & Unit names
            var created = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
            return MapToResponse(created!);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) return false;

            if (product.CostPrice != request.CostPrice)
            {
                _priceHistoryRepository.Add(new ProductPriceHistory
                {
                    ProductId = product.Id,
                    PriceType = PriceType.COST,
                    OldPrice = product.CostPrice,
                    NewPrice = request.CostPrice,
                    Note = "Cập nhật qua API"
                });
                product.CostPrice = request.CostPrice;
            }

            if (product.SellingPrice != request.SellingPrice)
            {
                _priceHistoryRepository.Add(new ProductPriceHistory
                {
                    ProductId = product.Id,
                    PriceType = PriceType.SELLING,
                    OldPrice = product.SellingPrice,
                    NewPrice = request.SellingPrice,
                    Note = "Cập nhật qua API"
                });
                product.SellingPrice = request.SellingPrice;
            }

            product.Sku = request.Sku;
            product.ProductName = request.ProductName;
            product.CategoryId = request.CategoryId;
            product.UnitId = request.UnitId;
            product.IsActive = request.IsActive;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) return false;

            // Guard: Cannot delete if product has inventory with quantity > 0
            var hasInventory = await _productRepository.HasInventoryAsync(id, cancellationToken);
            if (hasInventory)
            {
                throw new BusinessValidationException("Cannot delete product because it still has inventory stock (quantity > 0).");
            }

            // Guard: Cannot delete if product has stock transactions
            var hasTransactions = await _productRepository.HasStockTransactionsAsync(id, cancellationToken);
            if (hasTransactions)
            {
                throw new BusinessValidationException("Cannot delete product because it has associated stock transactions.");
            }

            // Guard: Cannot delete if product appears in any order items
            var hasOrderItems = await _productRepository.HasOrderItemsAsync(id, cancellationToken);
            if (hasOrderItems)
            {
                throw new BusinessValidationException("Cannot delete product because it is referenced in purchase or sales orders.");
            }

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Sku = product.Sku,
                ProductName = product.ProductName,
                CostPrice = product.CostPrice,
                SellingPrice = product.SellingPrice,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName ?? string.Empty,
                UnitId = product.UnitId,
                UnitName = product.Unit?.UnitName ?? string.Empty,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };
        }
    }
}
