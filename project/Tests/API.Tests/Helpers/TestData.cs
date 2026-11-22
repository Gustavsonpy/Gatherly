using API.Models;

namespace API.Tests.Helpers;

public static class TestData
{
    public static User User(string name = "Usuário Teste", string? email = null, string? city = "Curitiba") => new()
    {
        Name = name,
        Email = email ?? $"{Guid.NewGuid():N}@email.com",
        Birthday = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        HashPassword = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 4),
        City = city,
        RegisterDate = DateTime.UtcNow
    };

    public static Category Category(string name = "Esportes") => new() { Name = name };

    public static Event Event(
        Guid userId,
        Guid categoryId,
        DateTime? start = null,
        DateTime? end = null,
        string title = "Futebol",
        string city = "Curitiba",
        int maxCapacity = 10,
        DateTime? canceledAt = null,
        string? state = null)
    {
        var s = start ?? DateTime.UtcNow.AddDays(1);
        return new Event
        {
            Title = title,
            Description = "Partida amistosa",
            DateTimeStart = s,
            DateTimeEnd = end ?? s.AddHours(2),
            Localization = "Parque Barigui",
            MaxCapacity = maxCapacity,
            City = city,
            State = state,
            UrlImage = "http://localhost/uploads/events/img.jpg",
            UserId = userId,
            CategoryId = categoryId,
            CanceledAt = canceledAt,
            RegisterDate = DateTime.UtcNow
        };
    }

    public static EventParticipant Participant(Guid eventId, Guid userId, DateTime? leftAt = null, DateTime? registerDate = null) => new()
    {
        EventId = eventId,
        UserId = userId,
        LeftAt = leftAt,
        RegisterDate = registerDate ?? DateTime.UtcNow
    };
}
