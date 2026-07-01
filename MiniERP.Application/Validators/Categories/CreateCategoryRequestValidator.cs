using FluentValidation;
using MiniERP.Application.DTOs.Categories;
using MiniERP.Application.Interfaces.Repositories;

namespace MiniERP.Application.Validators.Categories
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryRequestValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
                .MustAsync(BeUniqueName).WithMessage("Category name already exists.");

            RuleFor(x => x.ParentId)
                .MustAsync(ParentMustExist!).WithMessage("Parent category does not exist.")
                .When(x => x.ParentId.HasValue);
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _categoryRepository.AnyAsync(c => c.CategoryName == name, cancellationToken);
        }

        private async Task<bool> ParentMustExist(int? parentId, CancellationToken cancellationToken)
        {
            if (!parentId.HasValue) return true;
            var parent = await _categoryRepository.GetByIdAsync(parentId.Value, cancellationToken);
            return parent != null;
        }
    }
}
