using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

/// <summary>An entry in an assignment's work log / activity stream.</summary>
public class AssignmentUpdate : Entity
{
    private AssignmentUpdate() { }

    internal AssignmentUpdate(Guid assignmentId, string author, string body, DateTime whenUtc)
    {
        AssignmentId = assignmentId;
        Author = author;
        Body = body;
        CreatedAtUtc = whenUtc;
    }

    public Guid AssignmentId { get; private set; }
    public string Author { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
}
