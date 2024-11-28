using System.Globalization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints;

public static class ErrorEndpoints
{
    public static void MapErrorEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/error", ReturnError);
    }

    private static IResult ReturnError()
    {
        return Results.BadRequest(new { message = "dumb dev. fix url." });
    }
}
