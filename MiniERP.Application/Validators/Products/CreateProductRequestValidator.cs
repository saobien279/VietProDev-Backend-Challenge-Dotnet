using FluentValidation;
using MiniERP.Application.DTOs.Products;
using MiniERP.Application.Interfaces.Repositories;

namespace MiniERP.Application.Validators.Products
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;

        public CreateProductRequestValidator(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUnitRepository unitRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitRepository = unitRepository;

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
                .MustAsync(BeUniqueSku).WithMessage("SKU already exists.");

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cost price must be >= 0.");

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Selling price must be >= 0.");

            RuleFor(x => x.CategoryId)
                .MustAsync(CategoryMustExist).WithMessage("Category does not exist.");

            RuleFor(x => x.UnitId)
                .MustAsync(UnitMustExist).WithMessage("Unit does not exist.");
        }

        private async Task<bool> BeUniqueSku(string sku, CancellationToken ct)
        {
            return !await _productRepository.AnyAsync(p => p.Sku == sku, ct);
        }

        private async Task<bool> CategoryMustExist(int categoryId, CancellationToken ct)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId, ct);
            return category != null;
        }

        private async Task<bool> UnitMustExist(int unitId, CancellationToken ct)
        {
            var unit = await _unitRepository.GetByIdAsync(unitId, ct);
            return unit != null;
        }
    }
}
