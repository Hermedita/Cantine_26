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
    }
}