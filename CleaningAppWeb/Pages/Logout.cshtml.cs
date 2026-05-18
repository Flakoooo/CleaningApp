using CleaningAppWeb.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleaningAppWeb.Pages
{
    public class LogoutModel(SignInManager<User> signInManager) : PageModel
    {
        private readonly SignInManager<User> _signInManager = signInManager;

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/login");
        }
    }
}
