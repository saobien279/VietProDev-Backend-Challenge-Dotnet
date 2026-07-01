using FluentValidation;
using MiniERP.Application.DTOs.SalesOrders;

namespace MiniERP.Application.Validators.SalesOrders
{
    public class SalesOrderItemDtoValidator : AbstractValidator<SalesOrderItemDto>
    {
        public SalesOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
