
using API.Common;
using API.Controllers;
using API.DTO.Category;
using API.Interfaces.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _service = new();

    private CategoryController CreateController() =>
        new(_service.Object, Mock.Of<ILogger<CategoryController>>());

    [Fact]
    public async Task GetAll_DeveRetornarOk_QuandoConsultaTemSucesso()
    {
        var categories = new List<CategoryDTO>
        {
            new() { Name = "Esportes" },
            new() { Name = "Jogos" }
        };

        _service.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<List<CategoryDTO>>.Success(categories));

        var result = await CreateController().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(categories, ok.Value);
    }

    [Fact]
    public async Task GetAll_DeveRetornarBadRequest_QuandoConsultaFalha()
    {
        _service.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<List<CategoryDTO>>.Failure("Erro"));

        var result = await CreateController().GetAll();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_DeveRetornarOk_QuandoCategoriaEhCriada()
    {
        var category = new CategoryDTO { Name = "Esportes" };

        _service.Setup(s => s.CreateAsync(It.IsAny<CreateCategoryDTO>()))
            .ReturnsAsync(Result<CategoryDTO>.Success(category));

        var result = await CreateController().Create(
            new CreateCategoryDTO { Name = "Esportes" });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(category, ok.Value);
    }

    [Fact]
    public async Task Create_DeveRetornarBadRequest_QuandoCategoriaJaExiste()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateCategoryDTO>()))
            .ReturnsAsync(
                Result<CategoryDTO>.Failure("Categoria duplicada"));

        var result = await CreateController().Create(
            new CreateCategoryDTO { Name = "Esportes" });

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
