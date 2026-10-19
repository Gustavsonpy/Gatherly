
using System.Security.Claims;
using API.Interfaces;
using Microsoft.AspNetCore.Http;

namespace API.Tests;

public class CurrentUserServiceTests
{
    private static CurrentUserService CreateService(
        ClaimsPrincipal? principal)
    {
        var context = new DefaultHttpContext();

        if (principal is not null)
            context.User = principal;

        var accessor = new HttpContextAccessor
        {
            HttpContext = context
        };

        return new CurrentUserService(accessor);
    }

    [Fact]
    public void UserId_DeveRetornarId_QuandoExisteClaimNameIdentifier()
    {
        var expectedId = Guid.NewGuid();

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    expectedId.ToString())
            },
            authenticationType: "Test");

        var service = CreateService(new ClaimsPrincipal(identity));

        var actualId = service.UserId;

        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void UserId_DeveUsarClaimSub_QuandoNameIdentifierNaoExiste()
    {
        var expectedId = Guid.NewGuid();

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("sub", expectedId.ToString())
            },
            authenticationType: "Test");

        var service = CreateService(new ClaimsPrincipal(identity));

        var actualId = service.UserId;

        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void UserId_DeveRetornarGuidVazio_QuandoClaimNaoExiste()
    {
        var service = CreateService(
            new ClaimsPrincipal(new ClaimsIdentity()));

        var actualId = service.UserId;

        Assert.Equal(Guid.Empty, actualId);
    }

    [Fact]
    public void Email_DeveRetornarEmail_QuandoClaimExiste()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Email, "maria@example.com")
            },
            authenticationType: "Test");

        var service = CreateService(new ClaimsPrincipal(identity));

        var email = service.Email;

        Assert.Equal("maria@example.com", email);
    }

    [Fact]
    public void IsAuthenticated_DeveRetornarTrue_QuandoIdentidadeEstaAutenticada()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            },
            authenticationType: "Test");

        var service = CreateService(new ClaimsPrincipal(identity));

        var authenticated = service.IsAuthenticated;

        Assert.True(authenticated);
    }

    [Fact]
    public void IsAuthenticated_DeveRetornarFalse_QuandoNaoHaIdentidadeAutenticada()
    {
        var service = CreateService(
            new ClaimsPrincipal(new ClaimsIdentity()));

        var authenticated = service.IsAuthenticated;

        Assert.False(authenticated);
    }
}
