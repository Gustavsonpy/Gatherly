using API.Common;
using API.Controllers;
using API.DTO;
using API.DTO.Category;
using API.DTO.Comment;
using API.DTO.Event;
using API.DTO.Rate;
using API.DTO.Report;
using API.DTO.User;
using API.Interfaces;
using API.Interfaces.Category;
using API.Interfaces.Comment;
using API.Interfaces.Event;
using API.Interfaces.Rate;
using API.Interfaces.Report;
using API.Services;
using API.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Controllers;

public class EventControllerTests
{
    private readonly Mock<IEventService> _service = new();
    private readonly Mock<IImageStorage> _storage = new();
    private readonly Guid _userId = Guid.NewGuid();

    private EventController Create(Guid? userId) =>
        new EventController(_service.Object, _storage.Object, NullLogger<EventController>.Instance).WithUser(userId);

    private static Result<T> Fail<T>() => Result<T>.Failure("erro");

    [Fact]
    public async Task Create_SemUsuario_RetornaUnauthorized()
    {
        Assert.IsType<UnauthorizedResult>(await Create(null).Create(new CreateEventDTO()));
    }

    [Fact]
    public async Task Create_SucessoEFalha()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateEventDTO>(), _userId)).ReturnsAsync(Result<EventDTO>.Success(new EventDTO()));
        Assert.IsType<OkObjectResult>(await Create(_userId).Create(new CreateEventDTO()));

        _service.Setup(s => s.CreateAsync(It.IsAny<CreateEventDTO>(), _userId)).ReturnsAsync(Fail<EventDTO>());
        Assert.Equal(new[] { "erro" }, (await Create(_userId).Create(new CreateEventDTO())).Errors());
    }

    [Fact]
    public async Task EndpointsDeConsulta_RetornamOkOuBadRequest()
    {
        var id = Guid.NewGuid();
        var list = Result<List<ReturnEventDTO>>.Success(new());
        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(list);
        _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(Result<ReturnEventDTO?>.Success(new ReturnEventDTO()));
        _service.Setup(s => s.GetMyCityEventsAsync()).ReturnsAsync(list);
        _service.Setup(s => s.GetParticipantsAsync(id)).ReturnsAsync(Result<List<ParticipantDTO>>.Success(new()));
        _service.Setup(s => s.GetMyEventsAsync()).ReturnsAsync(list);
        _service.Setup(s => s.GetFutureEvents()).ReturnsAsync(list);
        var c = Create(_userId);

        Assert.IsType<OkObjectResult>(await c.GetAll());
        Assert.IsType<OkObjectResult>(await c.GetEventById(id));
        Assert.IsType<OkObjectResult>(await c.GetMyCityEvents());
        Assert.IsType<OkObjectResult>(await c.GetParticipants(id));
        Assert.IsType<OkObjectResult>(await c.GetMyEvents());
        Assert.IsType<OkObjectResult>(await c.GetFutureEvents());

        var fail = Fail<List<ReturnEventDTO>>();
        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(fail);
        _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(Fail<ReturnEventDTO?>());
        _service.Setup(s => s.GetMyCityEventsAsync()).ReturnsAsync(fail);
        _service.Setup(s => s.GetParticipantsAsync(id)).ReturnsAsync(Fail<List<ParticipantDTO>>());
        _service.Setup(s => s.GetMyEventsAsync()).ReturnsAsync(fail);
        _service.Setup(s => s.GetFutureEvents()).ReturnsAsync(fail);

        Assert.IsType<BadRequestObjectResult>(await c.GetAll());
        Assert.IsType<BadRequestObjectResult>(await c.GetEventById(id));
        Assert.IsType<BadRequestObjectResult>(await c.GetMyCityEvents());
        Assert.IsType<BadRequestObjectResult>(await c.GetParticipants(id));
        Assert.IsType<BadRequestObjectResult>(await c.GetMyEvents());
        Assert.IsType<BadRequestObjectResult>(await c.GetFutureEvents());
    }

    [Fact]
    public async Task Acoes_RetornamNoContentOuBadRequest()
    {
        var id = Guid.NewGuid();
        var ok = Result<bool>.Success(true);
        _service.Setup(s => s.JoinAsync(id)).ReturnsAsync(ok);
        _service.Setup(s => s.LeaveAsync(id)).ReturnsAsync(ok);
        _service.Setup(s => s.CancelAsync(id)).ReturnsAsync(ok);
        var c = Create(_userId);

        Assert.IsType<NoContentResult>(await c.Join(id));
        Assert.IsType<NoContentResult>(await c.Leave(id));
        Assert.IsType<NoContentResult>(await c.Cancel(id));

        _service.Setup(s => s.JoinAsync(id)).ReturnsAsync(Fail<bool>());
        _service.Setup(s => s.LeaveAsync(id)).ReturnsAsync(Fail<bool>());
        _service.Setup(s => s.CancelAsync(id)).ReturnsAsync(Fail<bool>());

        Assert.IsType<BadRequestObjectResult>(await c.Join(id));
        Assert.IsType<BadRequestObjectResult>(await c.Leave(id));
        Assert.IsType<BadRequestObjectResult>(await c.Cancel(id));
    }

    [Fact]
    public async Task UploadImage_ArquivoInvalido_RetornaBadRequestSemSalvar()
    {
        var result = await Create(_userId).UploadImage(null!);

        Assert.Equal(new[] { "Nenhum arquivo enviado" }, result.Errors());
        _storage.Verify(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task UploadImage_ArquivoValido_SalvaNaPastaEvents()
    {
        _storage.Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), "events", default)).ReturnsAsync("http://x/uploads/events/a.jpg");

        var result = Assert.IsType<OkObjectResult>(await Create(_userId).UploadImage(FormFileFactory.ValidJpeg()));

        Assert.Contains("uploads/events/a.jpg", result.Value!.ToString());
    }
}

public class UserControllerTests
{
    private readonly Mock<IUserService> _service = new();
    private readonly Mock<IImageStorage> _storage = new();
    private readonly Guid _userId = Guid.NewGuid();

    private UserController Create(Guid? userId) => new UserController(_service.Object, _storage.Object).WithUser(userId);

    [Fact]
    public async Task Create_RetornaOkOuBadRequestComFieldErrors()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUserDTO>())).ReturnsAsync(Result<UserDTO>.Success(new UserDTO()));
        Assert.IsType<OkObjectResult>(await Create(null).Create(new CreateUserDTO()));

        var fields = new Dictionary<string, List<string>> { ["email"] = new() { "Já existe" } };
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUserDTO>())).ReturnsAsync(Result<UserDTO>.Failure(fields));
        var bad = Assert.IsType<BadRequestObjectResult>(await Create(null).Create(new CreateUserDTO()));
        Assert.Same(fields, bad.Value!.GetType().GetProperty("fieldErrors")!.GetValue(bad.Value));
    }

    [Fact]
    public async Task GetAllEGetById()
    {
        var id = Guid.NewGuid();
        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(Result<List<UserPublicDTO>>.Success(new()));
        _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(Result<UserPublicDTO?>.Success(new UserPublicDTO()));
        Assert.IsType<OkObjectResult>(await Create(null).GetAll());
        Assert.IsType<OkObjectResult>(await Create(null).GetById(id));

        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(Result<List<UserPublicDTO>>.Failure("x"));
        _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(Result<UserPublicDTO?>.Failure("x"));
        Assert.IsType<BadRequestObjectResult>(await Create(null).GetAll());
        Assert.IsType<BadRequestObjectResult>(await Create(null).GetById(id));
    }

    [Fact]
    public async Task EndpointsMe_SemUsuario_RetornamUnauthorized()
    {
        var c = Create(null);
        Assert.IsType<UnauthorizedResult>(await c.GetMe());
        Assert.IsType<UnauthorizedResult>(await c.UpdateMe(new UpdateUserDTO()));
        Assert.IsType<UnauthorizedResult>(await c.UploadPhoto(FormFileFactory.ValidJpeg()));
    }

    [Fact]
    public async Task GetMeEUpdateMe()
    {
        _service.Setup(s => s.GetMeAsync(_userId)).ReturnsAsync(Result<UserDTO>.Success(new UserDTO()));
        _service.Setup(s => s.UpdateMeAsync(_userId, It.IsAny<UpdateUserDTO>())).ReturnsAsync(Result<UserDTO>.Success(new UserDTO()));
        Assert.IsType<OkObjectResult>(await Create(_userId).GetMe());
        Assert.IsType<OkObjectResult>(await Create(_userId).UpdateMe(new UpdateUserDTO()));

        _service.Setup(s => s.GetMeAsync(_userId)).ReturnsAsync(Result<UserDTO>.Failure("x"));
        _service.Setup(s => s.UpdateMeAsync(_userId, It.IsAny<UpdateUserDTO>())).ReturnsAsync(Result<UserDTO>.Failure("x"));
        Assert.IsType<BadRequestObjectResult>(await Create(_userId).GetMe());
        Assert.IsType<BadRequestObjectResult>(await Create(_userId).UpdateMe(new UpdateUserDTO()));
    }

    [Fact]
    public async Task UploadPhoto_ArquivoInvalido_RetornaBadRequest()
    {
        var result = await Create(_userId).UploadPhoto(FormFileFactory.Create(FormFileFactory.PngHeader, "a.gif", "image/gif"));
        Assert.Equal(new[] { "Formato de imagem inválido" }, result.Errors());
    }

    [Fact]
    public async Task UploadPhoto_UsuarioInexistente_NaoSalvaArquivo()
    {
        _service.Setup(s => s.GetMeAsync(_userId)).ReturnsAsync(Result<UserDTO>.Failure("Usuário não encontrado"));

        var result = await Create(_userId).UploadPhoto(FormFileFactory.ValidJpeg());

        Assert.IsType<BadRequestObjectResult>(result);
        _storage.Verify(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task UploadPhoto_FalhaAoAtualizar_ApagaArquivoNovo()
    {
        _service.Setup(s => s.GetMeAsync(_userId)).ReturnsAsync(Result<UserDTO>.Success(new UserDTO { UrlPhoto = "antiga" }));
        _storage.Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), "users", default)).ReturnsAsync("nova");
        _service.Setup(s => s.UpdatePhotoAsync(_userId, "nova")).ReturnsAsync(Result<UserDTO>.Failure("x"));

        var result = await Create(_userId).UploadPhoto(FormFileFactory.ValidJpeg());

        Assert.IsType<BadRequestObjectResult>(result);
        _storage.Verify(s => s.DeleteAsync("nova", default), Times.Once);
        _storage.Verify(s => s.DeleteAsync("antiga", default), Times.Never);
    }

    [Fact]
    public async Task UploadPhoto_Sucesso_ApagaFotoAntiga()
    {
        _service.Setup(s => s.GetMeAsync(_userId)).ReturnsAsync(Result<UserDTO>.Success(new UserDTO { UrlPhoto = "antiga" }));
        _storage.Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), "users", default)).ReturnsAsync("nova");
        _service.Setup(s => s.UpdatePhotoAsync(_userId, "nova")).ReturnsAsync(Result<UserDTO>.Success(new UserDTO { UrlPhoto = "nova" }));

        var result = await Create(_userId).UploadPhoto(FormFileFactory.ValidJpeg());

        Assert.IsType<OkObjectResult>(result);
        _storage.Verify(s => s.DeleteAsync("antiga", default), Times.Once);
    }
}

public class SmallControllersTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CategoryController()
    {
        var service = new Mock<ICategoryService>();
        var c = new CategoryController(service.Object, NullLogger<CategoryController>.Instance);

        service.Setup(s => s.CreateAsync(It.IsAny<CreateCategoryDTO>())).ReturnsAsync(Result<CategoryDTO>.Success(new CategoryDTO()));
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(Result<List<CategoryDTO>>.Success(new()));
        Assert.IsType<OkObjectResult>(await c.Create(new CreateCategoryDTO()));
        Assert.IsType<OkObjectResult>(await c.GetAll());

        service.Setup(s => s.CreateAsync(It.IsAny<CreateCategoryDTO>())).ReturnsAsync(Result<CategoryDTO>.Failure("x"));
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(Result<List<CategoryDTO>>.Failure("x"));
        Assert.IsType<BadRequestObjectResult>(await c.Create(new CreateCategoryDTO()));
        Assert.IsType<BadRequestObjectResult>(await c.GetAll());
    }

    [Fact]
    public async Task CommentController()
    {
        var service = new Mock<ICommentService>();
        var c = new CommentController(service.Object);
        var eventId = Guid.NewGuid();

        service.Setup(s => s.createAsync(It.IsAny<CreateCommentDTO>())).ReturnsAsync(Result<CommentDTO>.Success(new CommentDTO()));
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(Result<List<CommentDTO>>.Success(new()));
        service.Setup(s => s.GetCommentsByEvent(eventId)).ReturnsAsync(Result<List<CommentDTO>>.Success(new()));
        Assert.IsType<OkObjectResult>(await c.Create(new CreateCommentDTO()));
        Assert.IsType<OkObjectResult>(await c.GetAll());
        Assert.IsType<OkObjectResult>(await c.GetCommentByEvent(eventId));

        service.Setup(s => s.createAsync(It.IsAny<CreateCommentDTO>())).ReturnsAsync(Result<CommentDTO>.Failure("x"));
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(Result<List<CommentDTO>>.Failure("x"));
        service.Setup(s => s.GetCommentsByEvent(eventId)).ReturnsAsync(Result<List<CommentDTO>>.Failure("x"));
        Assert.IsType<BadRequestObjectResult>(await c.Create(new CreateCommentDTO()));
        Assert.IsType<BadRequestObjectResult>(await c.GetAll());
        Assert.IsType<BadRequestObjectResult>(await c.GetCommentByEvent(eventId));
    }

    [Fact]
    public async Task UserRatingController()
    {
        var service = new Mock<IUserRatingService>();
        Guid? sentReporter = null;
        service.Setup(s => s.CreateRatingAsync(It.IsAny<Guid>(), It.IsAny<CreateUserRatingDTO>()))
            .Callback<Guid, CreateUserRatingDTO>((reporterId, _) => sentReporter = reporterId)
            .ReturnsAsync(Result<UserRatingDTO>.Success(new UserRatingDTO()));
        service.Setup(s => s.GetRatingByUser(_userId)).ReturnsAsync(new List<UserRatingDTO>());
        service.Setup(s => s.GetUserScore(_userId)).ReturnsAsync(new UserScoreDTO());

        Assert.IsType<UnauthorizedResult>(await new UserRatingController(service.Object).WithUser(null).Create(new CreateUserRatingDTO()));

        var c = new UserRatingController(service.Object).WithUser(_userId);
        // Quem avalia sempre vem do token, nunca do corpo da requisição.
        Assert.IsType<OkObjectResult>(await c.Create(new CreateUserRatingDTO()));
        Assert.Equal(_userId, sentReporter);
        Assert.IsType<OkObjectResult>(await c.GetByUser(_userId));
        Assert.IsType<OkObjectResult>(await c.GetScore(_userId));

        service.Setup(s => s.CreateRatingAsync(It.IsAny<Guid>(), It.IsAny<CreateUserRatingDTO>())).ReturnsAsync(Result<UserRatingDTO>.Failure("x"));
        Assert.IsType<BadRequestObjectResult>(await c.Create(new CreateUserRatingDTO()));
    }

    [Fact]
    public async Task UserReportController()
    {
        var service = new Mock<IUserReportService>();
        Guid? sentReporter = null;
        service.Setup(s => s.CreateReport(It.IsAny<Guid>(), It.IsAny<CreateReportDTO>()))
            .Callback<Guid, CreateReportDTO>((reporterId, _) => sentReporter = reporterId)
            .ReturnsAsync(Result<UserReportDTO>.Success(new UserReportDTO()));

        Assert.IsType<UnauthorizedResult>(await new UserReportController(service.Object).WithUser(null).Create(new CreateReportDTO()));

        var c = new UserReportController(service.Object).WithUser(_userId);
        // Quem denuncia sempre vem do token, nunca do corpo da requisição.
        Assert.IsType<OkObjectResult>(await c.Create(new CreateReportDTO()));
        Assert.Equal(_userId, sentReporter);

        service.Setup(s => s.CreateReport(It.IsAny<Guid>(), It.IsAny<CreateReportDTO>())).ReturnsAsync(Result<UserReportDTO>.Failure("x"));
        Assert.IsType<BadRequestObjectResult>(await c.Create(new CreateReportDTO()));
    }

    [Fact]
    public void ReportReasonController_RetornaMotivos()
    {
        var result = Assert.IsType<OkObjectResult>(new ReportReasonController(new ReportReasonService()).GetAll());
        Assert.NotEmpty((IEnumerable<API.DTOs.ReportReasonDTO>)result.Value!);
    }
}

public class AuthControllerTests : IDisposable
{
    private readonly API.Data.AppDbContext _db = TestDb.Create();

    private static IConfiguration Config(string? key = "chave-de-teste-com-pelo-menos-32-caracteres!!") =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = key,
            ["Jwt:Issuer"] = "gatherly-tests"
        }).Build();

    private AuthController Create(IConfiguration? config = null) => new(_db, config ?? Config());

    private API.Models.User Seed(Action<API.Models.User>? configure = null)
    {
        var user = TestData.User("Ana", "ana@email.com");
        configure?.Invoke(user);
        _db.Users.Add(user);
        _db.SaveChanges();
        return user;
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaToken()
    {
        Seed();

        var result = Assert.IsType<OkObjectResult>(await Create().Login(new AuthController.LoginRequest(" ANA@email.com ", "123456")));

        var token = (string)result.Value!.GetType().GetProperty("token")!.GetValue(result.Value)!;
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public async Task Login_EmailInexistente_RetornaUnauthorized()
    {
        Assert.IsType<UnauthorizedObjectResult>(await Create().Login(new("x@x.com", "123456")));
    }

    [Fact]
    public async Task Login_SenhaErrada_RetornaUnauthorized()
    {
        Seed();
        Assert.IsType<UnauthorizedObjectResult>(await Create().Login(new("ana@email.com", "errada")));
    }

    [Fact]
    public async Task Login_UsuarioSuspenso_RetornaUnauthorized()
    {
        Seed(u => u.IsSuspended = true);

        var result = Assert.IsType<UnauthorizedObjectResult>(await Create().Login(new("ana@email.com", "123456")));

        Assert.Contains("suspenso", result.Value!.ToString());
    }

    [Fact]
    public async Task Login_SemSenhaCadastrada_RetornaUnauthorized()
    {
        Seed(u => u.HashPassword = "");
        Assert.IsType<UnauthorizedObjectResult>(await Create().Login(new("ana@email.com", "123456")));
    }

    [Fact]
    public async Task Login_SemChaveJwt_Lanca()
    {
        Seed();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Create(Config(key: null)).Login(new("ana@email.com", "123456")));
    }
}
