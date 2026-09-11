namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>
/// The kind of a step, which drives what the onboarding wizard renders for it —
/// a specific form, an acknowledgement, or a plain checklist item. The engine
/// stores each step's captured data as JSON regardless of kind.
/// </summary>
public enum OnboardingStepKind
{
    /// <summary>A simple acknowledgement / checklist item with no data to capture.</summary>
    Generic = 0,

    /// <summary>Legal name, contact details, address.</summary>
    PersonalInfo = 1,

    /// <summary>Emergency contact name, relationship, phone.</summary>
    EmergencyContact = 2,

    /// <summary>Tax / payroll basics (filing status, allowances, SSN last-4).</summary>
    TaxPayroll = 3,

    /// <summary>Direct deposit bank details.</summary>
    DirectDeposit = 4,

    /// <summary>Read-and-accept a policy document.</summary>
    PolicyAcknowledgement = 5,

    /// <summary>Accounts / equipment / access the hire needs for their role.</summary>
    ITAccess = 6,

    /// <summary>Weekly availability (useful for hospitality / shift work).</summary>
    Availability = 7,
}
