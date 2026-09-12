using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding.Core;

/// <summary>
/// One person's journey through the Boundless core onboarding document (Part 1).
/// The hire reads each stage (an exact page of the onboarding document), types
/// their name and signs it; this aggregate records each stage they agreed to and
/// the final agreement to the document as a whole.
/// </summary>
public class CoreOnboardingProgress : AuditableEntity
{
    private readonly List<CoreOnboardingStageSignature> _signatures = new();

    private CoreOnboardingProgress() { }

    private CoreOnboardingProgress(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; private set; }

    /// <summary>Set once the hire agrees to the full document at the end of the flow.</summary>
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>The name typed on the final, whole-document agreement.</summary>
    public string? CompletedName { get; private set; }

    /// <summary>The signature entered on the final, whole-document agreement.</summary>
    public string? CompletedSignature { get; private set; }

    public IReadOnlyList<CoreOnboardingStageSignature> Signatures =>
        _signatures.OrderBy(s => s.StageOrder).ToList();

    public bool IsComplete => CompletedAtUtc is not null;

    public static CoreOnboardingProgress Start(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        return new CoreOnboardingProgress(userId);
    }

    /// <summary>
    /// Records (or re-records) the hire's agreement to one stage of the document.
    /// Re-signing a stage after a correction updates the existing record in place.
    /// </summary>
    public CoreOnboardingStageSignature SignStage(
        string stageKey, int stageOrder, string stageTitle,
        string typedName, string signature, DateTime whenUtc)
    {
        typedName = typedName.Trim();
        signature = signature.Trim();

        var existing = _signatures.FirstOrDefault(s => s.StageKey == stageKey);
        if (existing is not null)
        {
            existing.Resign(typedName, signature, whenUtc);
            // Editing a signed stage invalidates a prior whole-document agreement.
            CompletedAtUtc = null;
            CompletedName = null;
            CompletedSignature = null;
            return existing;
        }

        var created = new CoreOnboardingStageSignature(
            Id, stageKey, stageOrder, stageTitle, typedName, signature, whenUtc);
        _signatures.Add(created);
        return created;
    }

    /// <summary>
    /// Records the hire's agreement to the document as a whole. Every required
    /// stage must already be signed, otherwise the completion is rejected.
    /// </summary>
    public void CompleteDocument(
        string typedName, string signature,
        IReadOnlyCollection<string> requiredStageKeys, DateTime whenUtc)
    {
        var missing = requiredStageKeys
            .Where(k => _signatures.All(s => s.StageKey != k))
            .ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Every stage must be signed before agreeing to the whole document. Missing: {string.Join(", ", missing)}.");

        CompletedName = typedName.Trim();
        CompletedSignature = signature.Trim();
        CompletedAtUtc = whenUtc;
    }
}
