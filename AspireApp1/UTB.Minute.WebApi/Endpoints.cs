using UTB.Minute.Db;
using UTB.Minute.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

public static class WebAPI
{
    public static async Task<IResult> PrintMeals(MealDbContext db)
    {
        var meals = await db.Meals.ToListAsync();

        var mealDTOs = meals.Select(m => new MealDto
        {
            Id = m.MealId,
            Name = m.Name,
            Price = m.Price,
            Description = m.Description,
            IsActive = m.IsActive
        });

        return TypedResults.Ok(mealDTOs);
    }

    public static async Task<IResult> CreateNewMeal(MealDto newMealDTO,MealDbContext db)
    {

        if (newMealDTO.Name == null || newMealDTO.Price == null){
            return TypedResults.BadRequest("Jídlo musí mít název a cenu!");
        }

        var newMealEntity = new Meal
        {
            Name = newMealDTO.Name,
            Price = newMealDTO.Price,
            Description = "Nové jídelko yippee",
            IsActive = true 
        };

        db.Meals.Add(newMealEntity);

        await db.SaveChangesAsync();

        newMealDTO.Id = newMealEntity.MealId;
        newMealDTO.Description = newMealEntity.Description;
        newMealDTO.IsActive = true;

        return TypedResults.Created($"/meals/{newMealEntity.MealId}",newMealDTO);
    }

    public static async Task<IResult> UpdateMeal(int id, MealDto updatedMealDTO, MealDbContext db)
    {
        if (id != updatedMealDTO.Id)
        {
            return TypedResults.BadRequest("ID v URL se neshoduje s ID v těle požadavku");
        }

        if (updatedMealDTO.Name == null || updatedMealDTO.Price == null)
        {
            return TypedResults.BadRequest("Jídlo musí mít název a cenu!");
        }
        var existingMeal = await db.Meals.FindAsync(id);
        if (existingMeal == null)
        {
            return TypedResults.NotFound();
        }

        existingMeal.Name = updatedMealDTO.Name;
        existingMeal.Price = updatedMealDTO.Price;
        existingMeal.Description = updatedMealDTO.Description;
        existingMeal.IsActive = updatedMealDTO.IsActive;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeactivateMeal(int id, MealDbContext db)
    {
        var existingMeal = await db.Meals.FindAsync(id);
        if(existingMeal == null)
        {
            return TypedResults.NotFound();
        }

        existingMeal.IsActive = false;

        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> PrintMenus(MealDbContext db)
    {
        var menus = await db.MenuItems
            .Include(menu => menu.Meal)
            .ToListAsync();

        var menuDTOs = menus.Select(m => new MenuDto
        {
           Id = m.MenuId,
           Date = m.MenuDate,
           Portions = m.Portions,
           MealId = m.MealId,

           MealName = m.Meal?.Name ?? "Unknown name" 
        });
        
        return TypedResults.Ok(menuDTOs);
    }

    public static async Task<IResult> CreateNewMenu(MenuDto newMenuDTO, MealDbContext db)
    {
        if (newMenuDTO.Portions <= 0)
        {
            return TypedResults.BadRequest("Počet porcí musí být větší než 0!");
        }

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.MealId == newMenuDTO.MealId && m.IsActive);

        if (meal == null)
        {
            return TypedResults.BadRequest("Zvolené jídlo neexistuje!");
        }
        
        var newMenuEntity = new Menu
        {
            MealId = newMenuDTO.Id,
            MenuDate = newMenuDTO.Date,
            Portions = newMenuDTO.Portions  
        };

        db.MenuItems.Add(newMenuEntity);
        await db.SaveChangesAsync();

        newMenuDTO.Id = newMenuEntity.MenuId;
        newMenuDTO.MealName = meal.Name ?? "Unknown";

        return TypedResults.Created($"/menus/{newMenuEntity.MenuId}",newMenuDTO);
    }
}