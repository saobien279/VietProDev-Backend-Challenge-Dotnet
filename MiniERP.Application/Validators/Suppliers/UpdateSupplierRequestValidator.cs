using FluentValidation;
using MiniERP.Application.DTOs.Suppliers;
using MiniERP.Application.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Validators.Suppliers
{
    public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
    {
        private readonly ISupplierRepository _supplierRepository;

        public UpdateSupplierRequestValidator(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required for updates.");

            RuleFor(x => x.SupplierName)
                .NotEmpty().WithMessage("Supplier name is required.")
                .MaximumLength(100).WithMessage("Supplier name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .MustAsync(BeUniquePhoneForUpdate).WithMessage("Phone number already belongs to another supplier.");
        }

        private async Task<bool> BeUniquePhoneForUpdate(UpdateSupplierRequest model, string phone, CancellationToken cancellationToken)
        {
            return !await _supplierRepository.AnyAsync(s => s.Phone == phone && s.Id != model.Id, cancellationToken);
        }
    }
}