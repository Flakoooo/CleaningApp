using CleaningAppWeb.Data.Services;
using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CleaningAppWeb.Components.Shared.ApplicationModal
{
    public partial class ApplicationModal
    {
        [Inject]
        private AuthenticationStateProvider AuthProvider { get; set; } = null!;

        [Inject]
        private CleaningApplicationsService ApplicationsService { get; set; } = null!;

        [Parameter]
        public EventCallback OnModalClose { get; set; }

        [Parameter]
        public Guid? ApplicationId { get; set; }

        private string _modalState = "enter";

        private RoleType? _currentRole = null;

        private CleaningApplicationDTO? _application;

        protected override async Task OnInitializedAsync()
        {
            var user = (await AuthProvider.GetAuthenticationStateAsync()).User;

            if (Enum.TryParse(user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty, true, out RoleType role))
                _currentRole = role;

            if (ApplicationId.HasValue)
                _application = await ApplicationsService.GetApplicationAsync(ApplicationId.Value);
        }

        private async Task CloseModal()
        {
            if (OnModalClose.HasDelegate)
            {
                _modalState = "leave";
                await Task.Delay(100);
                await OnModalClose.InvokeAsync();
            }
        }

        private string GetStatusTranslation() => _application?.Status switch
        {
            CleaningApplicationStatus.Waiting => "Ожидание",
            CleaningApplicationStatus.InWork => "В работе",
            CleaningApplicationStatus.Done => "Выполнена",
            CleaningApplicationStatus.Rated => "Оценена",
            _ => _application?.Status.ToString() ?? "Статус"
        };

        private async Task UpdateApplicationStatus(CleaningApplicationStatus newStatus)
        {
            if (_application is not null)
            {
                var result = await ApplicationsService.UpdateApplicationStatus(_application.Id, newStatus);
                if (!result)
                {
                    return;
                }

                _application.Status = newStatus;
                if (newStatus is CleaningApplicationStatus.InWork)
                {
                    var user = (await AuthProvider.GetAuthenticationStateAsync()).User;
                    if (Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                    {
                        _application.Executor = new UserDTO
                        {
                            Id = userId,
                            FirstName = user.FindFirst("FirstName")?.Value ?? string.Empty,
                            LastName = user.FindFirst("LastName")?.Value ?? string.Empty,
                            Patronymic = user.FindFirst("Patronymic")?.Value ?? string.Empty
                        };
                    }
                    ;
                }

                StateHasChanged();
            }
        }
    }
}
