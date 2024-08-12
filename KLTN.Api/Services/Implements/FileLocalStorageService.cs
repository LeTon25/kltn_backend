using KLTN.Api.Services.Interfaces;

namespace KLTN.Api.Services.Implements
{
    public class FileLocalStorageService : IStorageService
    {
        private readonly string _userContentFolder;
        private const string USER_CONTENT_FOLDER_NAME = "attachments";
        public FileLocalStorageService(IWebHostEnvironment webHostEnvironment) 
        {
            _userContentFolder = Path.Combine(webHostEnvironment.WebRootPath, USER_CONTENT_FOLDER_NAME);
        }
        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_userContentFolder, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }

        public string GetFileUrl(string fileName)
        {
            return $"/{USER_CONTENT_FOLDER_NAME}/{fileName}";
        }

        public async Task SaveFileAsync(Stream mediaBinaryStream,string? filePath, string fileName)
        {
            var pathToSaveFile = this._userContentFolder;
            if (!string.IsNullOrEmpty(filePath)) { 
                pathToSaveFile = Path.Combine(pathToSaveFile, filePath);
            }
            var check = Directory.Exists(pathToSaveFile);
            if (!check)
                Directory.CreateDirectory(pathToSaveFile);

            var realFilePath = Path.Combine(pathToSaveFile, fileName);
            using var output = new FileStream(realFilePath, FileMode.Create);
            await mediaBinaryStream.CopyToAsync(output);
        }
    }
}
