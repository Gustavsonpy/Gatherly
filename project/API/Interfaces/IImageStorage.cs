namespace API.Interfaces
{
    public interface IImageStorage
    {
        Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default);
        Task DeleteAsync(string? url, CancellationToken ct = default);
    }
}