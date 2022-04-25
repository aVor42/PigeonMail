namespace PigeonMail.Services
{
    public interface IFileStorer
    {
        Task<string> AddFile(IFormFile file);

        byte[] GetFile(string filePath);

        byte[] GetDefaultFile(string filePath);

        string GetFilePath(string fileName);
    }
}
