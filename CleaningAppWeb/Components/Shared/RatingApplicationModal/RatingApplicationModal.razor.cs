using CleaningAppWeb.Domain.Requests;
using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.RatingApplicationModal
{
    public partial class RatingApplicationModal
    {
        [Parameter]
        public EventCallback OnModalClose { get; set; }

        [Parameter]
        public EventCallback<RatingApplicationRequest> OnApplicationRated { get; set; }

        private string _modalState = "enter";

        private RatingApplicationRequest RatingRequest { get; set; } = new();

        private async Task Close()
        {
            _modalState = "leave";
            await Task.Delay(100);
            await OnModalClose.InvokeAsync();
        }

        private async Task CloseModal()
        {
            if (OnModalClose.HasDelegate)
                await Close();
        }

        private void SelectRating(byte newRating) => RatingRequest.Rating = newRating;

        private async Task RateApplication()
        {
            if (RatingRequest.Rating == 0) return;

            if (OnApplicationRated.HasDelegate && OnModalClose.HasDelegate)
            {
                await Close();
                await OnApplicationRated.InvokeAsync(RatingRequest);
            }
        }
    }
}
