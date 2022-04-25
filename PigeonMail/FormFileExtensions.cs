namespace PigeonMail
{
    public static class FormFileExtensions
    {
        public static async Task<byte[]> GetBytes(this IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            return stream.ToArray();
        }

        public static IFormFile GetFormFile(this byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", "fileName");
        }
    }
}
