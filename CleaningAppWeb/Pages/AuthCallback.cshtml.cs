using CleaningAppWeb.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleaningAppWeb.Pages
{
    public class AuthCallbackModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager
    ) : PageModel
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return Redirect("/login");

            await _signInManager.SignInAsync(user, false);
            return Redirect("/");
        }
    }
}
