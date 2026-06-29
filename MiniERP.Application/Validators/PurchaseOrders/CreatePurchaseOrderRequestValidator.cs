using FluentValidation;
using MiniERP.Application.DTOs.PurchaseOrders;

namespace MiniERP.Application.Validators.PurchaseOrders
{
    public class CreatePurchaseOrderRequestValidator : AbstractValidator<CreatePurchaseOrderRequest>
    {
        public CreatePurchaseOrderRequestValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("Supplier ID is required.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Purchase order must contain at least one item.");

            RuleForEach(x => x.Items)
                .SetValidator(new PurchaseOrderItemDtoValidator());
        }
    }
}
