using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Work;

namespace BoundlessEnterprises.Application.Work;

/// <summary>Authorization helper for ring work: platform admins and a ring's holder can manage it.</summary>
public static class WorkAccess
{
    public static bool CanManage(ICurrentUser user, Ring ring) =>
        user.IsPlatformAdmin || (user.UserId is Guid uid && ring.HolderUserId == uid);
}
