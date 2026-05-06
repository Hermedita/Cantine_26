using UTB.Minute.Contracts;

namespace UTB.Minute.AdminClient
{
    public class CanteenService(HttpClient httpClient)
    {
        public async Task<MealDto[]?> GetMealsAsync()
        {
            MealDto[]? meals = await httpClient.GetFromJsonAsync<MealDto[]>("/meals");
            return meals;
        } 

        public async Task CreateMealAsync(MealRequestDto meal)
        {
            var response = await httpClient.PostAsJsonAsync("/meals", meal);
            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeMealStateAsync(MealStateRequestDto mealStateRequest, int id)
        {
            var response = await httpClient.PatchAsJsonAsync($"/meals/{id}/state", mealStateRequest);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateMealAsync(MealRequestDto meal, int id)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/meals/{id}", meal);
            response.EnsureSuccessStatusCode();
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
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/menus/{id}", menu);
            response.EnsureSuccessStatusCode();
        }

        public async Task CreateMenuAsync(MenuRequestDto menu)
        {
            var response = await httpClient.PostAsJsonAsync("/menus", menu);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteMenuAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"/menus/{id}");

            response.EnsureSuccessStatusCode();
        }


    }
    
}