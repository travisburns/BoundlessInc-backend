using BoundlessEnterprises.Application.Common.Interfaces;

namespace BoundlessEnterprises.Infrastructure.Common;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
