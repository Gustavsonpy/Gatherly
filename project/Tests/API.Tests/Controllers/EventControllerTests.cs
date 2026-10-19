
using System.Security.Claims;
using API.Controllers;
using API.DTO.Event;
using API.Interfaces.Event;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using API.Common;

namespace API.Tests.Controllers
{
    public class EventControllerTests
    {
        private readonly Mock<IEventService> _eventServiceMock = new();
        private readonly Mock<ILogger<EventController>> _loggerMock = new();

        private EventController CreateController(
            ClaimsPrincipal? user = null)
        {
            var controller = new EventController(
                _eventServiceMock.Object,
                _loggerMock.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user ?? new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(
                                    ClaimTypes.NameIdentifier,
                                    Guid.NewGuid().ToString()
                                )
                            },
                            "TestAuthentication"
                        )
                    )
                }
            };

            return controller;
        }

        // 1. Criar evento sem claim de usuário
        [Fact]
        public async Task Create_DeveRetornarUnauthorized_QuandoClaimNaoExiste()
        {
            var controller = CreateController(
                new ClaimsPrincipal(new ClaimsIdentity())
            );

            var dto = new CreateEventDTO();

            var result = await controller.Create(dto);

            Assert.IsType<UnauthorizedResult>(result);
        }

        // 2. Criar evento quando o serviço retorna falha
        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoServicoFalhar()
        {
            var controller = CreateController();
            var dto = new CreateEventDTO();

            _eventServiceMock
                .Setup(s => s.CreateAsync(
                    dto,
                    It.IsAny<Guid>()
                ))
                .ReturnsAsync(Result<EventDTO>.Failure("Erro ao criar evento"));

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // 3. Criar evento com sucesso
        [Fact]
        public async Task Create_DeveRetornarOk_QuandoCriacaoForBemSucedida()
        {
            var controller = CreateController();
            var dto = new CreateEventDTO();

            var eventDto = new EventDTO();

            _eventServiceMock
                .Setup(s => s.CreateAsync(
                    dto,
                    It.IsAny<Guid>()
                ))
                .ReturnsAsync(Result<EventDTO>.Success(eventDto));

            var result = await controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // 4. Listar eventos com sucesso
        [Fact]
        public async Task GetAll_DeveRetornarOk_QuandoServicoRetornarEventos()
        {
            var controller = CreateController();

            var events = new List<ReturnEventDTO>();

            _eventServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(Result<List<ReturnEventDTO>>.Success(events));

            var result = await controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        // 5. Listar eventos quando o serviço falha
        [Fact]
        public async Task GetAll_DeveRetornarBadRequest_QuandoServicoFalhar()
        {
            var controller = CreateController();

            _eventServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(
                    Result<List<ReturnEventDTO>>.Failure("Erro ao buscar eventos")
                );

            var result = await controller.GetAll();

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // 6. Participar de evento com sucesso
        [Fact]
        public async Task Join_DeveRetornarNoContent_QuandoParticipacaoForBemSucedida()
        {
            var controller = CreateController();
            var eventId = Guid.NewGuid();

            _eventServiceMock
                .Setup(s => s.JoinAsync(eventId))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await controller.Join(eventId);

            Assert.IsType<NoContentResult>(result);
        }

        // 7. Participar de evento quando o serviço falha
        [Fact]
        public async Task Join_DeveRetornarBadRequest_QuandoServicoFalhar()
        {
            var controller = CreateController();
            var eventId = Guid.NewGuid();

            _eventServiceMock
                .Setup(s => s.JoinAsync(eventId))
                .ReturnsAsync(Result<bool>.Failure("Evento lotado."));

            var result = await controller.Join(eventId);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // 8. Sair de evento com sucesso
        [Fact]
        public async Task Leave_DeveRetornarNoContent_QuandoSaidaForBemSucedida()
        {
            var controller = CreateController();
            var eventId = Guid.NewGuid();

            _eventServiceMock
                .Setup(s => s.LeaveAsync(eventId))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await controller.Leave(eventId);

            Assert.IsType<NoContentResult>(result);
        }

        // 9. Sair de evento quando o serviço falha
        [Fact]
        public async Task Leave_DeveRetornarBadRequest_QuandoServicoFalhar()
        {
            var controller = CreateController();
            var eventId = Guid.NewGuid();

            _eventServiceMock
                .Setup(s => s.LeaveAsync(eventId))
                .ReturnsAsync(Result<bool>.Failure(
                    "Você não participa deste evento."
                ));

            var result = await controller.Leave(eventId);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
