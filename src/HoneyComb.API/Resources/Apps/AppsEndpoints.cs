
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HoneyComb.API.Resources.Apps;
public static class AppsEndpoints
{
    public static IEndpointRouteBuilder MapAppsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/Apps", () =>
        {
            var apps = new List<AppResource>{
                new("app-a","应用A"),
                new("app-b","应用B"),
                new("app-c","应用C"),
                new("app-d","应用D"),
                new("app-e","应用E"),
                new("app-f","应用F"),
                new("app-g","应用G"),
            };

            return Results.Ok(new PaginatedList<AppResource>(apps, apps.Count, 1, 20));
        })
        .WithName("Apps.List")
        .WithTags("Apps");
        return endpoints;
    }

}

public record AppResource(string Key, string DisplayName);
