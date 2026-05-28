using UTB.Minute.Contracts;
using System.Net.Http.Json;

namespace UTB.Minute.AdminClient
{
    public class CanteenService
    {
        private readonly IHttpClientFactory _clientFactory;

        public CanteenService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient GetPublicClient() => _clientFactory.CreateClient("PublicApiClient");
        private HttpClient GetSecureClient() => _clientFactory.CreateClient("SecureApiClient");

        // --- VEŘEJNÉ METODY (Smazáno úvodní lomítko) ---

        public async Task<MealDto[]?> GetMealsAsync()
        {
            // Změněno z "/meals" na "meals"
            return await GetPublicClient().GetFromJsonAsync<MealDto[]>("meals");
        }

        public async Task<MealDto?> GetMealAsync(int id)
        {
            // Změněno z "/meals/{id}" na "meals/{id}"
            return await GetPublicClient().GetFromJsonAsync<MealDto>($"meals/{id}");
        }

        public async Task<MenuDto[]?> GetMenusAsync()
        {
            // Změněno z "/menus" na "menus"
            return await GetPublicClient().GetFromJsonAsync<MenuDto[]>("menus");
        }

        public async Task<MenuDto?> GetMenuAsync(int id)
        {
            // Změněno z "/menus/{id}" na "menus/{id}"
            return await GetPublicClient().GetFromJsonAsync<MenuDto>($"menus/{id}");
        }

        public async Task<OrderDto[]?> GetOrdersAsync()
        {
            // Změněno z "/orders" na "orders"
            return await GetPublicClient().GetFromJsonAsync<OrderDto[]>("orders");
        }

        public async Task CreateOrderAsync(int menuId)
        {
            // Změněno z "/orders" na "orders"
            var response = await GetPublicClient().PostAsJsonAsync("orders", new { MenuId = menuId });
            response.EnsureSuccessStatusCode();
        }

        // --- CHRÁNĚNÉ METODY (Smazáno úvodní lomítko) ---

        public async Task CreateMealAsync(MealRequestDto meal)
        {
            var response = await GetSecureClient().PostAsJsonAsync("meals", meal);
            response.EnsureSuccessStatusCode();

            await AutoNotifyChangesAsync();
        }

        public async Task ChangeMealStateAsync(MealStateRequestDto mealStateRequest, int id)
        {
            var response = await GetSecureClient().PatchAsJsonAsync($"meals/{id}/state", mealStateRequest);
            response.EnsureSuccessStatusCode();

            await AutoNotifyChangesAsync();
        }

        public async Task UpdateMealAsync(MealRequestDto meal, int id)
        {
            HttpResponseMessage response = await GetSecureClient().PutAsJsonAsync($"meals/{id}", meal);
            response.EnsureSuccessStatusCode();

            await AutoNotifyChangesAsync();
        }

        public async Task UpdateMenuAsync(MenuRequestDto menu, int id)
        {
            var response = await GetSecureClient().PutAsJsonAsync($"menus/{id}", menu);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.IsNullOrWhiteSpace(errorMessage) ? "Menu could not be updated." : errorMessage.Trim('"'));
            }
            await AutoNotifyChangesAsync();
        }

        public async Task CreateMenuAsync(MenuRequestDto menu)
        {
            var response = await GetSecureClient().PostAsJsonAsync("menus", menu);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.IsNullOrWhiteSpace(errorMessage) ? "Menu could not be created." : errorMessage.Trim('"'));
            }

            await AutoNotifyChangesAsync();
        }

        public async Task DeleteMenuAsync(int id)
        {
            var response = await GetSecureClient().DeleteAsync($"menus/{id}");
            response.EnsureSuccessStatusCode();
            await AutoNotifyChangesAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatusRequestDto request)
        {
            var response = await GetSecureClient().PutAsJsonAsync($"orders/{orderId}/status", request);
            response.EnsureSuccessStatusCode();
        }

        private async Task AutoNotifyChangesAsync()
        {
            try
            {
                await NotifyOrderUpdateAsync(0, 0);
            }
            catch
            {
                // Selhání notifikace neshodí uložení dat v administraci
            }
        }

        public async Task NotifyOrderUpdateAsync(int totalPortions, decimal totalPrice)
        {
            var message = new OrderUpdateNotificationDto
            {
                TotalPortions = totalPortions,
                TotalPrice = totalPrice
            };
            var response = await GetSecureClient().PostAsJsonAsync("api/orders/notify-change", message);
            response.EnsureSuccessStatusCode();
        }
    }

    public class OrderUpdateNotificationDto
    {
        public int TotalPortions { get; set; }
        public decimal TotalPrice { get; set; }
    }
}