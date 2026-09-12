using BoundlessEnterprises.Domain.Onboarding.Core;

namespace BoundlessEnterprises.Application.Onboarding.Core.DTOs;

/// <summary>A signing stage of the document, as presented to the hire.</summary>
public sealed record CoreOnboardingStageDto(
    string Key, int Order, int PageNumber, string Title, string Subtitle, string ImageUrl);

/// <summary>A recorded stage agreement (the per-stage audit line).</summary>
public sealed record CoreOnboardingSignatureDto(
    string StageKey, int StageOrder, string StageTitle, string TypedName, DateTime SignedAtUtc)
{
    public static CoreOnboardingSignatureDto FromEntity(CoreOnboardingStageSignature s) =>
        new(s.StageKey, s.StageOrder, s.StageTitle, s.TypedName, s.SignedAtUtc);
}

/// <summary>The current user's progress through the document.</summary>
public sealed record CoreOnboardingProgressDto(
    IReadOnlyList<string> SignedStageKeys,
    IReadOnlyList<CoreOnboardingSignatureDto> Signatures,
    bool IsComplete,
    DateTime? CompletedAtUtc,
    string? CompletedName)
{
    public static CoreOnboardingProgressDto Empty { get; } =
        new(Array.Empty<string>(), Array.Empty<CoreOnboardingSignatureDto>(), false, null, null);

    public static CoreOnboardingProgressDto FromEntity(CoreOnboardingProgress p) =>
        new(
            p.Signatures.Select(s => s.StageKey).ToList(),
            p.Signatures.Select(CoreOnboardingSignatureDto.FromEntity).ToList(),
            p.IsComplete,
            p.CompletedAtUtc,
            p.CompletedName);
}

/// <summary>The whole document — its stages plus the caller's progress through it.</summary>
public sealed record CoreOnboardingDocumentDto(
    string Version,
    IReadOnlyList<CoreOnboardingStageDto> Stages,
    CoreOnboardingProgressDto Progress);
