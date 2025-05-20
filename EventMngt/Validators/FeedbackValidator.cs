using FluentValidation;
using EventMngt.DTOs;

namespace EventMngt.Validators;

public class CreateFeedbackDTOValidator : AbstractValidator<CreateFeedbackDTO>
{
    public CreateFeedbackDTOValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID must be greater than 0");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
    }
}

public class UpdateFeedbackDTOValidator : AbstractValidator<UpdateFeedbackDTO>
{
    public UpdateFeedbackDTOValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
    }
} 