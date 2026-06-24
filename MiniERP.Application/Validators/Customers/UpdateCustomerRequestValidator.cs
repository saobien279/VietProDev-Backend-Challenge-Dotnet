using FluentValidation;
using MiniERP.Application.DTOs.Customers;
using MiniERP.Application.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Validators.Customers
{
    public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
    {
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerRequestValidator(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
        }
    }
}