using FluentValidation;
using KnowledgeSpace.ViewModels.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgeSpace.ViewModels.Validators
{
    public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
    {
        public UserCreateRequestValidator()
        {
            //// NAME IS REQUIRED
            RuleFor(x => x.UserName).NotEmpty().WithMessage("User name is required");

            //// PASSWORD IS REQUIRED AND HAVE MINLENGTH IS 8, HAVE UPPER CHARACTER + NUMBER + SPECIAL CHARACTER
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password has to at least 8 characters")
                .Matches(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")
                .WithMessage("Password is not match complexity rules.");

            //// EMAIL IS REQUIRED AND MUST MATCH FORMAT
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .Matches(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").WithMessage("Email format is not match");

            ////PHONE NUMBER IS REQUIRED
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required");

            //// FIRST NAME IS REQUIRED AND HAVE MAXLENGTH IS 50
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name can not over 50 characters limit");
            
            //// LAST NAME IS REQUIRED AND HAVE MAXLENGTH IS 50
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name can not over 50 characters limit");
        }
    }
}
