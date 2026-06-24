using MiniERP.Application.DTOs.Categories;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            return categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                ParentId = c.ParentId,
                ParentCategoryName = c.ParentCategory?.CategoryName,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var category = new Category
            {
                CategoryName = request.CategoryName,
                ParentId = request.ParentId
            };

            _categoryRepository.Add(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);

            return new CategoryResponse
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                ParentId = category.ParentId,
                ParentCategoryName = null, // Parent not loaded after insert; acceptable for create response
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null) return false;

            category.CategoryName = request.CategoryName;
            category.ParentId = request.ParentId;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null) return false;

            // Guard: Cannot delete if category has products
            var hasProducts = await _categoryRepository.HasProductsAsync(id, cancellationToken);
            if (hasProducts)
            {
                throw new InvalidOperationException("Cannot delete category because it has associated products.");
            }

            // Guard: Cannot delete if category has sub-categories
            var hasSubCategories = await _categoryRepository.AnyAsync(c => c.ParentId == id, cancellationToken);
            if (hasSubCategories)
            {
                throw new InvalidOperationException("Cannot delete category because it has sub-categories.");
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
