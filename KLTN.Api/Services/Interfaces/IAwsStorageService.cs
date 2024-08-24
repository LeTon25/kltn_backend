namespace KLTN.Api.Services.Interfaces
{
    public interface IAwsStorageService
    {
        Task<string> SaveFileAsync(Stream mediaBinaryStream, string? filePath, string fileName);

        Task DeleteFileAsync(string fileName);
    }
}
