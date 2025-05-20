using FluentValidation;
using EventMngt.DTOs;

namespace EventMngt.Validators;

public class CreateRegistrationDTOValidator : AbstractValidator<CreateRegistrationDTO>
{
    public CreateRegistrationDTOValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID must be greater than 0");
    }
}

public class UpdateRegistrationDTOValidator : AbstractValidator<UpdateRegistrationDTO>
{
    public UpdateRegistrationDTOValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => 
                status == "Pending" || 
                status == "Confirmed" || 
                status == "Cancelled" || 
                status == "Rejected")
            .WithMessage("Status must be one of: Pending, Confirmed, Cancelled, Rejected");
    }
} 