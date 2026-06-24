using FluentValidation;
using MiniERP.Application.DTOs.Units;
using MiniERP.Application.Interfaces.Repositories;

namespace MiniERP.Application.Validators.Units
{
    public class CreateUnitRequestValidator : AbstractValidator<CreateUnitRequest>
    {
        private readonly IUnitRepository _unitRepository;

        public CreateUnitRequestValidator(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;

            RuleFor(x => x.UnitName)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.")
                .MustAsync(BeUniqueName).WithMessage("Unit name already exists.");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _unitRepository.AnyAsync(u => u.UnitName == name, cancellationToken);
        }
    }
}
