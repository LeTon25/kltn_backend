namespace KLTN.Api.Services.Interfaces
{
    public interface IStorageService
    {
        string GetFileUrl(string fileName);

        Task SaveFileAsync(Stream mediaBinaryStream,string? filePath, string fileName);

        Task DeleteFileAsync(string fileName);
    }
}
