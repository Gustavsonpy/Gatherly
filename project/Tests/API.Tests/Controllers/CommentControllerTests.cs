
using API.Common;
using API.Controllers;
using API.DTO.Comment;
using API.Interfaces.Comment;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class CommentControllerTests
{
    private readonly Mock<ICommentService> _service = new();

    private CommentController CreateController() =>
        new(_service.Object);

    [Fact]
    public async Task GetAll_DeveRetornarOk_QuandoConsultaTemSucesso()
    {
        var comments = new List<CommentDTO>
        {
            new() { Text = "Vou participar!" }
        };

        _service.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<List<CommentDTO>>.Success(comments));

        var result = await CreateController().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(comments, ok.Value);
    }

    [Fact]
    public async Task GetAll_DeveRetornarBadRequest_QuandoConsultaFalha()
    {
        _service.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<List<CommentDTO>>.Failure("Erro"));

        var result = await CreateController().GetAll();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetCommentByEvent_DeveRetornarComentariosDoEvento()
    {
        var eventId = Guid.NewGuid();
        var comments = new List<CommentDTO>
        {
            new() { EventId = eventId, Text = "Até lá!" }
        };

        _service.Setup(s => s.getCommentsByEvent(eventId))
            .ReturnsAsync(Result<List<CommentDTO>>.Success(comments));

        var result = await CreateController().GetCommentByEvent(eventId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(comments, ok.Value);
    }

    [Fact]
    public async Task Create_DeveRetornarBadRequest_QuandoComentarioFalha()
    {
        _service.Setup(s => s.createAsync(It.IsAny<CreateCommentDTO>()))
            .ReturnsAsync(
                Result<CommentDTO>.Failure("Usuário não autenticado"));

        var result = await CreateController().Create(
            new CreateCommentDTO { Text = "Olá!" });

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
