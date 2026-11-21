using System.Security.Claims;
using API.Enums;
using API.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests.Services;

public class ReportReasonServiceTests
{
    [Fact]
    public void GetAll_RetornaUmItemParaCadaMotivoDoEnum()
    {
        var reasons = new ReportReasonService().GetAll().ToList();

        Assert.Equal(Enum.GetValues<ReportReason>().Select(r => (int)r), reasons.Select(r => r.Id));
        Assert.All(reasons, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.Name));
            Assert.False(string.IsNullOrWhiteSpace(r.Icon));
        });
    }
}

public class CurrentUserServiceTests
{
    private static CurrentUserService Create(params Claim[] claims)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, claims.Length > 0 ? "Test" : null))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns(context);
        return new CurrentUserService(accessor.Object);
    }

    [Fact]
    public void UserId_LeClaimNameIdentifier()
    {
        var id = Guid.NewGuid();
        Assert.Equal(id, Create(new Claim(ClaimTypes.NameIdentifier, id.ToString())).UserId);
    }

    [Fact]
    public void UserId_UsaClaimSubComoAlternativa()
    {
        var id = Guid.NewGuid();
        Assert.Equal(id, Create(new Claim("sub", id.ToString())).UserId);
    }

    [Fact]
    public void UserId_ClaimInvalidaOuAusente_RetornaGuidVazio()
    {
        Assert.Equal(Guid.Empty, Create(new Claim(ClaimTypes.NameIdentifier, "abc")).UserId);
        Assert.Equal(Guid.Empty, Create().UserId);
    }

    [Fact]
    public void EmailEIsAuthenticated_RefletemOUsuario()
    {
        var service = Create(new Claim(ClaimTypes.Email, "a@a.com"));
        Assert.Equal("a@a.com", service.Email);
        Assert.True(service.IsAuthenticated);

        var anonymous = Create();
        Assert.Null(anonymous.Email);
        Assert.False(anonymous.IsAuthenticated);
    }

    [Fact]
    public void SemHttpContext_RetornaValoresPadrao()
    {
        var service = new CurrentUserService(new Mock<IHttpContextAccessor>().Object);
        Assert.Equal(Guid.Empty, service.UserId);
        Assert.Null(service.Email);
        Assert.False(service.IsAuthenticated);
    }
}
