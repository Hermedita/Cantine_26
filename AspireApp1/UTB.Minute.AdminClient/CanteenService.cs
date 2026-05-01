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
    }
    
}