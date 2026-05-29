using UTB.Minute.Contracts;
using System.Net.Http.Json;

namespace UTB.Minute.AdminClient
{
    public class CanteenService(HttpClient httpClient)
    {
        public async Task<MealDto[]?> GetMealsAsync()
        {
            try
            {
                MealDto[]? meals = await httpClient.GetFromJsonAsync<MealDto[]>("/meals");
                return meals;
            }
            catch (TaskCanceledException ex) when (!System.Threading.CancellationToken.None.IsCancellationRequested)
            {
                // likely a timeout
                throw new HttpRequestException($"Request to backend timed out contacting {httpClient.BaseAddress}/meals", ex);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Failed to get meals from backend at {httpClient.BaseAddress}/meals: {ex.Message}", ex);
            }
        }

        public async Task CreateMealAsync(MealRequestDto meal)
        {
            var response = await httpClient.PostAsJsonAsync("/meals", meal);
            response.EnsureSuccessStatusCode();

            await AutoNotifyChangesAsync();
        }

        public async Task ChangeMealStateAsync(MealStateRequestDto mealStateRequest, int id)
        {
            var response = await httpClient.PatchAsJsonAsync($"/meals/{id}/state", mealStateRequest);
            response.EnsureSuccessStatusCode();

            await AutoNotifyChangesAsync();
        }

        public async Task UpdateMealAsync(MealRequestDto meal, int id)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/meals/{id}", meal);
            response.EnsureSuccessStatusCode();

            await AutoNotifyChangesAsync();
        }

        public async Task<MealDto?> GetMealAsync(int id)
        {
            MealDto? meal = await httpClient.GetFromJsonAsync<MealDto>($"/meals/{id}");
            return meal;
        }

        public async Task<MenuDto[]?> GetMenusAsync()
        {
            return await httpClient.GetFromJsonAsync<MenuDto[]>("/menus");
        }

        public async Task<MenuDto?> GetMenuAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<MenuDto>($"/menus/{id}");
        }

        public async Task UpdateMenuAsync(MenuRequestDto menu, int id)
        {
            var response = await httpClient.PutAsJsonAsync($"/menus/{id}", menu);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = "Menu could not be updated.";
                throw new HttpRequestException(errorMessage.Trim('"'));
            }
            await AutoNotifyChangesAsync();
        }

        public async Task CreateMenuAsync(MenuRequestDto menu)
        {
            var response = await httpClient.PostAsJsonAsync("/menus", menu);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = "Menu could not be created.";
                throw new HttpRequestException(errorMessage.Trim('"'));
            }

            await AutoNotifyChangesAsync();
        }

        public async Task DeleteMenuAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"/menus/{id}");

            response.EnsureSuccessStatusCode();
            await AutoNotifyChangesAsync();
        }

        private async Task AutoNotifyChangesAsync()
        {
            try
            {
                await NotifyOrderUpdateAsync(0, 0);
            }
            catch
            {
                // Selhání notifikace by nemělo shodit uložení dat v administraci
            }
        }

        public async Task NotifyOrderUpdateAsync(int totalPortions, decimal totalPrice)
        {
            var message = new OrderUpdateNotificationDto
            {
                TotalPortions = totalPortions,
                TotalPrice = totalPrice
            };

            var response = await httpClient.PostAsJsonAsync("/api/orders/notify-change", message);
            response.EnsureSuccessStatusCode();
        }
    }

    public class OrderUpdateNotificationDto
    {
        public int TotalPortions { get; set; }
        public decimal TotalPrice { get; set; }
    }
}