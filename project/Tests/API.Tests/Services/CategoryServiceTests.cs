using API.DTO.Category;
using API.Interfaces.Category;
using API.Models;
using API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private CategoryService CreateService() => new(_repo.Object, NullLogger<CategoryService>.Instance);

    [Fact]
    public async Task CreateAsync_NomeDuplicado_RetornaErro()
    {
        _repo.Setup(r => r.GetByName("Esportes")).ReturnsAsync(new Category { Name = "Esportes" });

        var result = await CreateService().CreateAsync(new CreateCategoryDTO { Name = "Esportes" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Already exists a category with this name", result.Errors.Single());
        _repo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NomeNovo_CriaCategoria()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

        var result = await CreateService().CreateAsync(new CreateCategoryDTO { Name = "Música" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Música", result.Value!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaErro()
    {
        var result = await CreateService().GetByIdAsync(Guid.NewGuid());
        Assert.Equal("Category not found", result.Errors.Single());
    }

    [Fact]
    public async Task GetByIdAsync_Existente_RetornaDto()
    {
        var category = new Category { Name = "Arte" };
        _repo.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);

        var result = await CreateService().GetByIdAsync(category.Id);

        Assert.Equal(category.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetAllAsync_RetornaTodas()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category> { new() { Name = "A" }, new() { Name = "B" } });

        var result = await CreateService().GetAllAsync();

        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task UpdateNameAsync_Inexistente_RetornaErro()
    {
        var result = await CreateService().UpdateNameAsync(Guid.NewGuid(), "x");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateNameAsync_Existente_AtualizaNome()
    {
        var category = new Category { Name = "Antigo" };
        _repo.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);

        var result = await CreateService().UpdateNameAsync(category.Id, "Novo");

        Assert.Equal("Novo", result.Value!.Name);
        _repo.Verify(r => r.UpdateAsync(category), Times.Once);
    }
}
