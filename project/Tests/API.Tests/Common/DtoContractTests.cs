using System.Text.Json;
using System.Text.Json.Nodes;
using API.DTO;
using API.DTO.Comment;
using API.DTO.Event;
using API.DTO.Rate;
using API.DTO.Report;

namespace API.Tests.Common;

/// <summary>
/// Garante que os DTOs de entrada recusam JSON sem os campos obrigatórios de tipo valor
/// (Guid, int, DateTime, enum). Sem isso, um campo faltando viraria Guid.Empty, 0 ou
/// 01/01/0001 sem nenhum aviso. A API usa as mesmas opções de JSON (JsonSerializerDefaults.Web).
/// </summary>
public class DtoContractTests
{
    private static readonly JsonSerializerOptions ApiJson = new(JsonSerializerDefaults.Web);

    private const string Id = "\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\"";

    private const string EventJson = $$"""
        {
          "title": "Futebol", "description": "Partida", "localization": "Parque", "city": "Joinville",
          "urlImage": "http://localhost/uploads/events/a.jpg", "maxCapacity": 10,
          "dateTimeStart": "2030-01-10T10:00:00Z", "dateTimeEnd": "2030-01-10T12:00:00Z",
          "categoryId": {{Id}}
        }
        """;

    private static readonly Dictionary<Type, string> ValidJson = new()
    {
        [typeof(CreateCommentDTO)] = $$"""{ "text": "Bora!", "eventId": {{Id}} }""",
        [typeof(CreateEventDTO)] = EventJson,
        [typeof(UpdateEventDTO)] = EventJson,
        [typeof(CreateUserRatingDTO)] = $$"""{ "rate": 5, "ratedUserId": {{Id}}, "eventId": {{Id}} }""",
        [typeof(CreateReportDTO)] = $$"""{ "reportedUserId": {{Id}}, "eventId": {{Id}}, "reason": 1 }""",
        [typeof(CreateEventReportDTO)] = $$"""{ "eventId": {{Id}}, "reason": 1 }""",
        [typeof(CreateUserDTO)] = """{ "name": "Ana", "email": "ana@email.com", "password": "123456", "birthday": "2000-01-01" }""",
        [typeof(UpdateUserDTO)] = """{ "name": "Ana", "birthday": "2000-01-01" }""",
    };

    public static TheoryData<Type> AllDtos() => new(ValidJson.Keys);

    public static TheoryData<Type, string> RequiredFields() => new()
    {
        { typeof(CreateCommentDTO), "eventId" },
        { typeof(CreateEventDTO), "dateTimeStart" },
        { typeof(CreateEventDTO), "dateTimeEnd" },
        { typeof(CreateEventDTO), "categoryId" },
        { typeof(UpdateEventDTO), "dateTimeStart" },
        { typeof(UpdateEventDTO), "dateTimeEnd" },
        { typeof(UpdateEventDTO), "maxCapacity" },
        { typeof(UpdateEventDTO), "categoryId" },
        { typeof(CreateUserRatingDTO), "rate" },
        { typeof(CreateUserRatingDTO), "ratedUserId" },
        { typeof(CreateUserRatingDTO), "eventId" },
        { typeof(CreateReportDTO), "reportedUserId" },
        { typeof(CreateReportDTO), "eventId" },
        { typeof(CreateReportDTO), "reason" },
        { typeof(CreateEventReportDTO), "eventId" },
        { typeof(CreateEventReportDTO), "reason" },
        { typeof(CreateUserDTO), "birthday" },
        { typeof(UpdateUserDTO), "birthday" },
    };

    [Theory]
    [MemberData(nameof(AllDtos))]
    public void JsonCompleto_EhAceito(Type dto)
    {
        Assert.NotNull(JsonSerializer.Deserialize(ValidJson[dto], dto, ApiJson));
    }

    [Theory]
    [MemberData(nameof(RequiredFields))]
    public void JsonSemCampoObrigatorio_EhRecusado(Type dto, string field)
    {
        var json = JsonNode.Parse(ValidJson[dto])!.AsObject();
        json.Remove(field);

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json.ToJsonString(), dto, ApiJson));
    }
}
