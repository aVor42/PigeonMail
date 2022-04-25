namespace PigeonMail.Services
{
    public class FileStorer : IFileStorer
    {
        public readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _appEnvironment;

        public FileStorer(IConfiguration configuration, IWebHostEnvironment appEnvironment)
        {
            _configuration = configuration;
            _appEnvironment = appEnvironment;
        }

        public async Task<string> AddFile(IFormFile file)
        {
            var path = GetFilePath(Guid.NewGuid().ToString());
            using var stream = File.Create(path);
            await file.CopyToAsync(stream);
            return path;
        }

        public byte[] GetFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public byte[] GetDefaultFile(string filePath)
        {
            var path = Path.Combine(_appEnvironment.ContentRootPath, $"wwwroot/{filePath}");
            return File.ReadAllBytes(path);
        }

        public string GetFilePath(string fileName)
        {
            return _configuration["StoreSongsPath"] + "/" + fileName + ".mp3";
        }

    }
}
