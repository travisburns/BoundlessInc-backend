namespace BoundlessEnterprises.Api.Endpoints;

/// <summary>
/// A capability's HTTP surface. Each module maps its own thin endpoints that
/// authenticate, validate, dispatch to an Application use case, and return.
/// </summary>
public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
