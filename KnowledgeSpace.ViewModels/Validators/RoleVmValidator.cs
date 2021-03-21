using FluentValidation;
using KnowledgeSpace.ViewModels.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgeSpace.ViewModels.Validators
{
    public class RoleVmValidator : AbstractValidator<RoleVm>
    {
        public RoleVmValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Role Id is required.")
                              .MaximumLength(50).WithMessage("Role Id is at most 50 character.");

            RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required.");
        }
    }
}
