using CleaningAppWeb.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace CleaningAppWeb.Components.Pages.Login
{
    [Route("/login")]
    public partial class Login
    {
        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        [Inject]
        private UserManager<User> UserManager { get; set; } = null!;

        private string _login = string.Empty;
        private string _password = string.Empty;
        private readonly Dictionary<string, string> _errors = [];

        private async Task LoginUser()
        {
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(_login))
                _errors.Add("login", "Поле не может быть пустым");
            if (string.IsNullOrWhiteSpace(_password))
                _errors.Add("password", "Поле не может быть пустым");

            if (_errors.Count > 0) return;

            var user = await UserManager.FindByNameAsync(_login);
            if (user is null || !await UserManager.CheckPasswordAsync(user, _password))
            {
                _errors.Add("login", "Неверный логин или пароль");
                _errors.Add("password", "Неверный логин или пароль");
                return;
            }

            var token = Guid.NewGuid().ToString();

            Navigation.NavigateTo($"/auth-callback?userId={user.Id}&token={token}", forceLoad: true);
        }
    }
}
