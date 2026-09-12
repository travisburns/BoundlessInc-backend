using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding.Core;

/// <summary>
/// A single stage of the Boundless core onboarding document that the hire has
/// read, typed their name against, and signed. One row per stage the person
/// agreed to — the per-stage half of the acknowledgement audit trail.
/// </summary>
public class CoreOnboardingStageSignature : Entity
{
    private CoreOnboardingStageSignature() { }

    internal CoreOnboardingStageSignature(
        Guid progressId, string stageKey, int stageOrder, string stageTitle,
        string typedName, string signature, DateTime signedAtUtc)
    {
        ProgressId = progressId;
        StageKey = stageKey;
        StageOrder = stageOrder;
        StageTitle = stageTitle;
        TypedName = typedName;
        Signature = signature;
        SignedAtUtc = signedAtUtc;
    }

    public Guid ProgressId { get; private set; }

    /// <summary>Stable key of the document stage (matches the document definition).</summary>
    public string StageKey { get; private set; } = string.Empty;

    /// <summary>Order the stage appears in the document, snapshotted for the record.</summary>
    public int StageOrder { get; private set; }

    /// <summary>Stage title, snapshotted so the record reads on its own.</summary>
    public string StageTitle { get; private set; } = string.Empty;

    /// <summary>The full name the hire typed when agreeing to this stage.</summary>
    public string TypedName { get; private set; } = string.Empty;

    /// <summary>The signature the hire entered for this stage.</summary>
    public string Signature { get; private set; } = string.Empty;

    public DateTime SignedAtUtc { get; private set; }

    internal void Resign(string typedName, string signature, DateTime signedAtUtc)
    {
        TypedName = typedName;
        Signature = signature;
        SignedAtUtc = signedAtUtc;
    }
}
