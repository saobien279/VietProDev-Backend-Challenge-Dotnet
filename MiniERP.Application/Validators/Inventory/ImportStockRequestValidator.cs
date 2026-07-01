using FluentValidation;
using MiniERP.Application.DTOs.Inventory;
using MiniERP.Application.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Validators.Inventory
{
    public class ImportStockRequestValidator : AbstractValidator<ImportStockRequest>
    {
        private readonly IProductRepository _productRepository;

        public ImportStockRequestValidator(IProductRepository productRepository)
        {
            _productRepository = productRepository;

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.")
                .MustAsync(ProductMustExist).WithMessage("Product does not exist or has been deleted.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
        }

        private async Task<bool> ProductMustExist(Guid productId, CancellationToken ct)
        {
            if (productId == Guid.Empty) return false;
            var product = await _productRepository.GetByIdAsync(productId, ct);
            return product != null;
        }
    }
}
