using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Application.Features.Roles.Commands
{
    public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        private readonly IRoleService _roleService;
        private readonly LocalizationHelper _localizer;

        public CreateRoleCommandValidator(IRoleService roleService, LocalizationHelper localizer)
        {
            _roleService = roleService;
            _localizer = localizer;

            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage(_localizer[RoleConsts.NameIsRequired])
                .Must(name => name.Length >= RoleConsts.NameMinLength)
                    .WithMessage(_localizer[RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()])
                .Must(name => name.Length <= RoleConsts.NameMaxLength)
                    .WithMessage(_localizer[RoleConsts.NameMustBeLessThanCharacters, RoleConsts.NameMaxLength.ToString()])
                .MustAsync(async (name, ct) => !await _roleService.RoleExistsAsync(name))
                    .WithMessage(_localizer[RoleConsts.NameExists]);
        }
    }
} 