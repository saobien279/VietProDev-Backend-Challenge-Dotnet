using FluentValidation;
using MiniERP.Application.DTOs.PurchaseOrders;

namespace MiniERP.Application.Validators.PurchaseOrders
{
    public class PurchaseOrderItemDtoValidator : AbstractValidator<PurchaseOrderItemDto>
    {
        public PurchaseOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");
        }
    }
}
