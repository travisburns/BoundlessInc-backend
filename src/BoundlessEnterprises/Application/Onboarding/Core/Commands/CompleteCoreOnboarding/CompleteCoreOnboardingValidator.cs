using FluentValidation;

namespace BoundlessEnterprises.Application.Onboarding.Core.Commands.CompleteCoreOnboarding;

public sealed class CompleteCoreOnboardingValidator : AbstractValidator<CompleteCoreOnboardingCommand>
{
    public CompleteCoreOnboardingValidator()
    {
        RuleFor(x => x.TypedName)
            .NotEmpty().WithMessage("Type your full name to agree to the document.")
            .MinimumLength(2)
            .MaximumLength(160);
        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("A signature is required to agree to the document.")
            .MaximumLength(160);
    }
}
