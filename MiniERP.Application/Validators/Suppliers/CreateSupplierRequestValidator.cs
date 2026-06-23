using FluentValidation;
using MiniERP.Application.DTOs.Suppliers;
using MiniERP.Application.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Validators.Suppliers
{
    public class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
    {
        private readonly ISupplierRepository _supplierRepository;

        public CreateSupplierRequestValidator(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;

            RuleFor(x => x.SupplierName)
                .NotEmpty().WithMessage("Supplier name is required.")
                .MaximumLength(100).WithMessage("Supplier name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .MustAsync(BeUniquePhone).WithMessage("Phone number already exists.");
        }

        private async Task<bool> BeUniquePhone(string phone, CancellationToken cancellationToken)
        {
            return !await _supplierRepository.AnyAsync(s => s.Phone == phone, cancellationToken);
        }
    }
}