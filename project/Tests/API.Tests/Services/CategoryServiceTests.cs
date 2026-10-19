
using API.DTO.Category;
using API.Interfaces.Category;
using API.Models;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repository = new();

    private CategoryService CreateService() =>
        new(_repository.Object, Mock.Of<ILogger<CategoryService>>());

    [Fact]
    public async Task CreateAsync_DeveCriarCategoria_QuandoNomeNaoExiste()
    {
        var dto = new CreateCategoryDTO { Name = "Esportes" };

        _repository.Setup(r => r.GetByName(dto.Name))
            .ReturnsAsync((Category?)null);

        _repository.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        var result = await CreateService().CreateAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Esportes", result.Value!.Name);

        _repository.Verify(
            r => r.AddAsync(It.Is<Category>(c => c.Name == "Esportes")),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoCategoriaJaExiste()
    {
        _repository.Setup(r => r.GetByName("Esportes"))
            .ReturnsAsync(new Category { Name = "Esportes" });

        var result = await CreateService().CreateAsync(
            new CreateCategoryDTO { Name = "Esportes" });

        Assert.False(result.IsSuccess);
        Assert.Contains(
            "Already exists a category with this name",
            result.Errors);

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_DeveFalhar_QuandoCategoriaNaoExiste()
    {
        var id = Guid.NewGuid();

        _repository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Category?)null);

        var result = await CreateService().GetByIdAsync(id);

        Assert.False(result.IsSuccess);
        Assert.Contains("Category not found", result.Errors);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarTodasAsCategorias()
    {
        _repository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category>
            {
                new() { Name = "Esportes" },
                new() { Name = "Jogos" },
                new() { Name = "Passeios" }
            });

        var result = await CreateService().GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Contains(result.Value, c => c.Name == "Jogos");
    }

    [Fact]
    public async Task UpdateNameAsync_DeveAtualizarNome_QuandoCategoriaExiste()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Futebol" };

        _repository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(category);

        var result = await CreateService().UpdateNameAsync(id, "Vôlei");

        Assert.True(result.IsSuccess);
        Assert.Equal("Vôlei", result.Value!.Name);

        _repository.Verify(
            r => r.UpdateAsync(It.Is<Category>(c =>
                c.Id == id && c.Name == "Vôlei")),
            Times.Once);
    }

    [Fact]
    public async Task UpdateNameAsync_DeveFalhar_QuandoCategoriaNaoExiste()
    {
        var id = Guid.NewGuid();

        _repository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Category?)null);

        var result = await CreateService().UpdateNameAsync(id, "Vôlei");

        Assert.False(result.IsSuccess);
        Assert.Contains("Category not found", result.Errors);

        _repository.Verify(
            r => r.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }
}
