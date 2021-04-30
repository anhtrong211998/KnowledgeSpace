using FluentValidation;
using KnowledgeSpace.ViewModels.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgeSpace.ViewModels.Validators
{
    public class FunctionCreateRequestValidator : AbstractValidator<FunctionCreateRequest>
    {
        public FunctionCreateRequestValidator()
        {
            //// ID IS REQUIRED AND HAVE MAXLENGTH IS 50
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id value is required")
               .MaximumLength(50).WithMessage("Function Id cannot over limit 50 characters");

            //// NAME IS REQUIRED AND HAVE MAXLENGTH IS 200
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name value is required")
              .MaximumLength(200).WithMessage("Name cannot over limit 200 characters");

            //// URL IS REQUIRED AND HAVE MAXLENGTH IS 200
            RuleFor(x => x.Url).NotEmpty().WithMessage("URL value is required")
             .MaximumLength(200).WithMessage("URL cannot over limit 200 characters");

            //// PARENT ID HAVE MAXLENGTH IS 50 WHEN PARENT ID NOT NULL OR EMPTY
            RuleFor(x => x.ParentId).MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.ParentId))
                .WithMessage("ParentId cannot over limit 50 characters");
        }
    }
}
