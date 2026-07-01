using FluentValidation;
using MiniERP.Application.DTOs.Units;
using MiniERP.Application.Interfaces.Repositories;

namespace MiniERP.Application.Validators.Units
{
    public class UpdateUnitRequestValidator : AbstractValidator<UpdateUnitRequest>
    {
        private readonly IUnitRepository _unitRepository;

        public UpdateUnitRequestValidator(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;

            RuleFor(x => x.UnitName)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.")
                .MustAsync((model, name, context, ct) => BeUniqueNameForUpdate(name, context, ct))
                .WithMessage("Unit name already exists.");
        }

        private async Task<bool> BeUniqueNameForUpdate(string name, ValidationContext<UpdateUnitRequest> context, CancellationToken ct)
        {
            if (!context.RootContextData.TryGetValue("Id", out var idObj) || idObj is not int id)
                return true;

            return !await _unitRepository.AnyAsync(u => u.UnitName == name && u.Id != id, ct);
        }
    }
}
