using FluentValidation;

namespace BoundlessEnterprises.Application.Onboarding.Core.Commands.SignCoreStage;

public sealed class SignCoreStageValidator : AbstractValidator<SignCoreStageCommand>
{
    public SignCoreStageValidator()
    {
        RuleFor(x => x.StageKey).NotEmpty();
        RuleFor(x => x.TypedName)
            .NotEmpty().WithMessage("Type your full name to sign.")
            .MinimumLength(2)
            .MaximumLength(160);
        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("A signature is required to agree to this section.")
            .MaximumLength(160);
    }
}
