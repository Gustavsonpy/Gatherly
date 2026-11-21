using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Tests.Integration;

/// <summary>
/// Sobe a API real em memória (Program.cs completo), trocando apenas o PostgreSQL
/// por um banco InMemory isolado por instância.
/// </summary>
public class GatherlyApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "integration-" + Guid.NewGuid();

    public GatherlyApiFactory()
    {
        // Program.cs lê essas chaves antes do Build, então precisam existir no ambiente.
        Environment.SetEnvironmentVariable("Jwt__Key", "chave-de-integracao-com-pelo-menos-32-caracteres");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "gatherly-tests");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options => options
                .UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    public void Seed(Action<AppDbContext> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seed(db);
        db.SaveChanges();
    }
}
