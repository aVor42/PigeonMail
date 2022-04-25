using Microsoft.AspNetCore.Mvc;
using PigeonMail.Data;
using PigeonMail.Services;

namespace PigeonMail.Controllers
{
    public class UsersController: Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IFileStorer _fileStorer;

        public UsersController(ApplicationDbContext context, IFileStorer fileStorer)
        {
            _context = context;
            _fileStorer = fileStorer;
        }


        [HttpGet]
        public FileContentResult GetPhoto(string id)
        {
            var user = _context.Users.Find(id);
            var bytes = user is not null ?
                user.Photo :
                _fileStorer.GetDefaultFile("img/img.png");
            return File(bytes, "image/jpeg", "user Photo.jpg");
        }

    }
}
