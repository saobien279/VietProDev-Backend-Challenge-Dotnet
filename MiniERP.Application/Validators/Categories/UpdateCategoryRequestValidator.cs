using FluentValidation;
using MiniERP.Application.DTOs.Categories;
using MiniERP.Application.Interfaces.Repositories;

namespace MiniERP.Application.Validators.Categories
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryRequestValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
                .MustAsync((model, name, context, ct) => BeUniqueNameForUpdate(name, context, ct))
                .WithMessage("Category name already exists.");

            RuleFor(x => x.ParentId)
                .MustAsync((model, parentId, context, ct) => ParentMustExistAndNotCircular(parentId, context, ct))
                .WithMessage("Invalid parent category: does not exist or would create a circular reference.")
                .When(x => x.ParentId.HasValue);
        }

        private async Task<bool> BeUniqueNameForUpdate(string name, ValidationContext<UpdateCategoryRequest> context, CancellationToken ct)
        {
            if (!context.RootContextData.TryGetValue("Id", out var idObj) || idObj is not int id)
                return true;

            return !await _categoryRepository.AnyAsync(c => c.CategoryName == name && c.Id != id, ct);
        }

        private async Task<bool> ParentMustExistAndNotCircular(int? parentId, ValidationContext<UpdateCategoryRequest> context, CancellationToken ct)
        {
            if (!parentId.HasValue) return true;

            if (!context.RootContextData.TryGetValue("Id", out var idObj) || idObj is not int currentId)
                return true;

            // Rule 1: Cannot set parent to itself
            if (parentId.Value == currentId) return false;

            // Rule 2: Parent must exist
            var parent = await _categoryRepository.GetByIdAsync(parentId.Value, ct);
            if (parent == null) return false;

            // Rule 3: Walk up the ancestor chain to detect circular reference
            // If any ancestor's ParentId == currentId → circular
            int? current = parent.ParentId;
            while (current.HasValue)
            {
                if (current.Value == currentId) return false; // Circular detected!

                var ancestor = await _categoryRepository.GetByIdAsync(current.Value, ct);
                if (ancestor == null) break; // Reached root (or broken chain)
                current = ancestor.ParentId;
            }

            return true;
        }
    }
}
