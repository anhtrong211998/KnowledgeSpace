using FluentValidation;
using KnowledgeSpace.ViewModels.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgeSpace.ViewModels.Validators
{
    public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
    {
        public RoleCreateRequestValidator()
        {
            //// ID IS REQUIRED AND HAVE MAXLENGTH IS 50
            RuleFor(x => x.Id).NotEmpty().WithMessage("Role Id is required.")
                              .MaximumLength(50).WithMessage("Role Id is at most 50 character.");

            //// NAME IS REQUIRED
            RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required.");
        }
    }
}
