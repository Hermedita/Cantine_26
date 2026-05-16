using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UTB.Minute.Contracts;

namespace UTB.Minute.CanteenClient;

public class CanteenService(HttpClient httpClient)
{
    public async Task<MenuDto[]?> GetMenusAsync()
    {
        return await httpClient.GetFromJsonAsync<MenuDto[]>("/menus");
    }

    public async Task<OrderDto[]?> GetOrdersAsync()
    {
        return await httpClient.GetFromJsonAsync<OrderDto[]>("/orders");
    }

    public async Task CreateOrderAsync(int menuId)
    {
        var request = new OrderRequestDto
        {
            MenuId = menuId
        };

        var response = await httpClient.PostAsJsonAsync("/orders", request);
        await EnsureSuccessOrThrowWarningAsync(response, "Order could not be created.");
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatusRequestDto request)
    {
        var response = await httpClient.PutAsJsonAsync($"/orders/{orderId}/status", request);
        await EnsureSuccessOrThrowWarningAsync(response, "Order status could not be changed.");
    }

    private static async Task EnsureSuccessOrThrowWarningAsync(
        HttpResponseMessage response,
        string fallbackMessage
    )
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string message = await ReadErrorMessageAsync(response, fallbackMessage);
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string fallbackMessage
    )
    {
        string content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return fallbackMessage;
        }

        try
        {
            string? stringMessage = JsonSerializer.Deserialize<string>(content);

            if (!string.IsNullOrWhiteSpace(stringMessage))
            {
                return stringMessage;
            }
        }
        catch (JsonException)
        {
            // Response is not a JSON string.
        }

        return content.Trim().Trim('"');
    }

        public async Task<MealDto[]?> GetMealsAsync()
        {
            MealDto[]? meals = await httpClient.GetFromJsonAsync<MealDto[]>("/meals");
            return meals;
        } 
        
}