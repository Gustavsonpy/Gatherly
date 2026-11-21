using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Helpers;

public static class ControllerTestExtensions
{
    /// <summary>Simula um usuário autenticado (ou anônimo quando userId é null).</summary>
    public static T WithUser<T>(this T controller, Guid? userId) where T : ControllerBase
    {
        var identity = userId is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }, "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    /// <summary>Extrai a lista "errors" do corpo de um BadRequestObjectResult.</summary>
    public static List<string> Errors(this IActionResult result)
    {
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var prop = bad.Value!.GetType().GetProperty("errors")!;
        return ((IEnumerable<string>)prop.GetValue(bad.Value)!).ToList();
    }
}
