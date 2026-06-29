using FluentValidation;
using MiniERP.Application.DTOs.SalesOrders;

namespace MiniERP.Application.Validators.SalesOrders
{
    public class CreateSalesOrderRequestValidator : AbstractValidator<CreateSalesOrderRequest>
    {
        public CreateSalesOrderRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Sales order must contain at least one item.");

            RuleForEach(x => x.Items)
                .SetValidator(new SalesOrderItemDtoValidator());
        }
    }
}
