using BoundlessEnterprises.Domain.Documents;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Documents;

public class DocumentTests
{
    [Fact]
    public void Create_StartsActive_Version1_AndPublished()
    {
        var doc = Document.Create(Guid.NewGuid(), "Handbook", DocumentType.Policy, "desc");

        Assert.Equal("Handbook", doc.Title);
        Assert.Equal(DocumentType.Policy, doc.Type);
        Assert.Equal(1, doc.Version);
        Assert.True(doc.IsActive);
    }

    [Fact]
    public void Update_BumpsVersion()
    {
        var doc = Document.Create(Guid.NewGuid(), "Handbook", DocumentType.Policy);
        doc.Update("Handbook v2", "updated", "https://example.com/doc");

        Assert.Equal(2, doc.Version);
        Assert.Equal("Handbook v2", doc.Title);
    }

    [Fact]
    public void Assignment_Acknowledge_SetsStatusAndTimestamp()
    {
        var assignment = DocumentAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal(AssignmentStatus.Assigned, assignment.Status);

        var now = DateTime.UtcNow;
        assignment.Acknowledge(now);

        Assert.Equal(AssignmentStatus.Acknowledged, assignment.Status);
        Assert.Equal(now, assignment.AcknowledgedAtUtc);
    }
}
