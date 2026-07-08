using aspnetapp.Services;
using Greet.V1;
using Microsoft.AspNetCore.Http.HttpResults;

namespace aspnetapp.Endpoints;

/// <summary>
/// Hello endpoints
/// </summary>
public static class HelloEndpoints
{
    /// <summary>
    /// Maps the versioned hello endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapHello(this WebApplication app)
    {
        var hello = app.NewVersionedApi("Hello");
        var v1 = hello.MapGroup("/v{version:apiVersion}/hello").HasApiVersion(1.0);

        v1.MapGet("/{name}", async Task<Results<Ok<HelloReply>, BadRequest>> (string name, IGreeterService service) =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    return TypedResults.BadRequest();
                }

                return TypedResults.Ok(await service.SayHello(new HelloRequest { Name = name }, null!));
            })
            .WithName("SayHello")
            .WithSummary("A simple method to say hello")
            .WithDescription("Sample request:\n\n    GET /hello/{name}")
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
