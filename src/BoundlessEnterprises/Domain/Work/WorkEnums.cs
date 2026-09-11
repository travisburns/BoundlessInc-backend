namespace BoundlessEnterprises.Domain.Work;

/// <summary>The thirteen domains (organs) of Boundless Enterprises. Each is held by one Ring.</summary>
public enum RingDomain
{
    World = 0,      // Worldbuilding & Story
    Image = 1,      // Art & Visual Design
    Resonance = 2,  // Music & Audio
    Machine = 3,    // Software & Engineering
    Crown = 4,      // Executive Leadership & Strategy
    Compass = 5,    // Product
    Engine = 6,     // Operations & Production Management
    Ledger = 7,     // Finance & Accounting
    Seal = 8,       // Legal & IP
    Hearth = 9,     // People & Talent
    Herald = 10,    // Marketing, Brand & PR
    Gate = 11,      // Sales, BD & Partnerships
    Circle = 12,    // Community & Customer Experience
}

/// <summary>The fundamental kind of work — cross-domain, not ring-specific.</summary>
public enum AssignmentType
{
    Create = 0,
    Build = 1,
    Research = 2,
    Review = 3,
    Decide = 4,
    Maintain = 5,
    Fix = 6,
    Deliver = 7,
    Plan = 8,
}

/// <summary>Assignment lifecycle: Assigned → InProgress → Review → Complete, with Blocked available.</summary>
public enum AssignmentStatus
{
    Draft = 0,
    Assigned = 1,
    InProgress = 2,
    Blocked = 3,
    Review = 4,
    Complete = 5,
    Archived = 6,
}

public enum AssignmentPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3,
}
