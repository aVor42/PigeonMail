using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PigeonMail.Models;

namespace PigeonMail.Controllers
{
    public class AccountController : Controller
    {

        private readonly SignInManager<User> _signInManager;

        public AccountController(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
