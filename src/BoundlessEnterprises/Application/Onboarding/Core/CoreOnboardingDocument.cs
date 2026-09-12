namespace BoundlessEnterprises.Application.Onboarding.Core;

/// <summary>One stage of the core onboarding document — an exact page the hire reads and signs.</summary>
public sealed record CoreOnboardingStageDef(
    string Key, int Order, int PageNumber, string Title, string Subtitle, string ImageUrl);

/// <summary>
/// The fixed definition of Boundless's core onboarding document (Part 1 of the
/// BoundlessIP onboarding). The content is the exact onboarding document: each
/// stage renders a rendered page image, and the hire types their name and signs
/// it. Pages 2–14 are the shared document every hire agrees to; page 1 is the
/// cover and page 15 is the person-specific appointment (handled in Part 3), so
/// neither is a signing stage here.
/// </summary>
public static class CoreOnboardingDocument
{
    /// <summary>Semantic version of the document. A change here means re-agreement is expected.</summary>
    public const string Version = "1.0";

    public static readonly IReadOnlyList<CoreOnboardingStageDef> Stages = new List<CoreOnboardingStageDef>
    {
        new("welcome",      1,  2, "Welcome to Boundless",            "What Is Boundless Enterprise",                        "/onboarding/core/page-02.jpg"),
        new("dream",        2,  3, "Boundless Dream",                 "The World and the Story at Its Center",               "/onboarding/core/page-03.jpg"),
        new("soul-skill",   3,  4, "Why You Are Here",                "The Soul Skill",                                      "/onboarding/core/page-04.jpg"),
        new("how-we-think", 4,  5, "How Boundless Thinks",            "Cause Before Ornament",                               "/onboarding/core/page-05.jpg"),
        new("canon",        5,  6, "Creation, Canon & Disagreement",  "Creation Is Not Canonization",                        "/onboarding/core/page-06.jpg"),
        new("rings",        6,  7, "The Thirteen Rings",              "Domains of Stewardship",                              "/onboarding/core/page-07.jpg"),
        new("tools",        7,  8, "The Mask, The Veil & The Quill",  "Tools of Power",                                      "/onboarding/core/page-08.jpg"),
        new("way-we-work",  8,  9, "The Way We Work",                 "Work, Wellbeing, Growth",                             "/onboarding/core/page-09.jpg"),
        new("expectations", 9, 10, "What We Expect From You",         "High Standards. A Stronger Tomorrow.",                "/onboarding/core/page-10.jpg"),
        new("ownership",   10, 11, "Your Work & Boundless",           "Creation. Credit. Ownership. Permission.",            "/onboarding/core/page-11.jpg"),
        new("first-horizon",11,12, "Your First Horizon",             "Your First Contribution Should Matter.",              "/onboarding/core/page-12.jpg"),
        new("building",    12, 13, "What We Are Building Together",    "A Longer Tomorrow.",                                  "/onboarding/core/page-13.jpg"),
        new("standard",    13, 14, "The Standard",                    "Higher People. Brighter Worlds.",                     "/onboarding/core/page-14.jpg"),
    };

    public static readonly IReadOnlyCollection<string> RequiredStageKeys =
        Stages.Select(s => s.Key).ToList();

    public static CoreOnboardingStageDef? FindStage(string key) =>
        Stages.FirstOrDefault(s => s.Key == key);
}
