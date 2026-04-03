using UTB.Minute.Db;
using UTB.Minute.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Formats.Asn1;

public static class WebAPI
{
    public static async Task<IResult> PrintMeals(MealDbContext db)
    {
        var meals = await db.Meals.ToListAsync();

        var mealDTOs = meals.Select(m => new MealDto
        {
            Id = m.MealId,
            Name = m.Name ?? string.Empty, 
            Price = m.Price ?? 0,          
            Description = m.Description,
            IsActive = m.IsActive
        });

        return TypedResults.Ok(mealDTOs);
    }

    public static async Task<IResult> CreateNewMeal(MealRequestDto request, MealDbContext db)
    {
        var newMealEntity = new Meal
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            IsActive = true 
        };

        db.Meals.Add(newMealEntity);
        await db.SaveChangesAsync();

        var responseDto = new MealDto
        {
            Id = newMealEntity.MealId,
            Name = newMealEntity.Name,
            Price = newMealEntity.Price ?? 0,
            Description = newMealEntity.Description,
            IsActive = newMealEntity.IsActive
        };

        return TypedResults.Created($"/meals/{newMealEntity.MealId}", responseDto);
    }

    public static async Task<IResult> UpdateMeal(int id, MealRequestDto request, MealDbContext db)
    {

        var existingMeal = await db.Meals.FindAsync(id);
        if (existingMeal == null)
        {
            return TypedResults.NotFound();
        }

        existingMeal.Name = request.Name;
        existingMeal.Price = request.Price;
        existingMeal.Description = request.Description;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> ChangeMealState(int id, MealStateRequestDto request, MealDbContext db)
    {
        var existingMeal = await db.Meals.FindAsync(id);
        if(existingMeal == null)
        {
            return TypedResults.NotFound();
        }

        existingMeal.IsActive = request.IsActive;

        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> PrintMenus(MealDbContext db)
    {
        var menus = await db.MenuItems
            .Include(menu => menu.Meal)
            .Where(menu => menu.Portions > 0 && menu.Meal!.IsActive == true)
            .ToListAsync();

        var menuDTOs = menus.Select(m => new MenuDto
        {
           Id = m.MenuId,
           Date = m.MenuDate,
           Portions = m.Portions,
           MealId = m.MealId,
           MealName = m.Meal?.Name ?? "Uknown name"
        });
        
        return TypedResults.Ok(menuDTOs);
    }

    public static async Task<IResult> CreateNewMenu(MenuRequestDto request, MealDbContext db)
{
    if (request.Portions <= 0)
    {
        return TypedResults.BadRequest("Počet porcí musí být větší než 0!");
    }

    var meal = await db.Meals.FirstOrDefaultAsync(m => m.MealId == request.MealId && m.IsActive);
    if (meal == null)
    {
        return TypedResults.BadRequest("Zvolené jídlo neexistuje nebo není aktivní!");
    }


    var duplicateMenu = await db.MenuItems.FirstOrDefaultAsync(m => m.MealId == request.MealId && m.MenuDate == request.Date);
    if (duplicateMenu != null)
    {
    
        return TypedResults.BadRequest("Tohle jídlo už je na tento den naplánované! Pokud chcete více porcí, upravte existující menu.");
    }


    var newMenuEntity = new Menu
    {
        MealId = request.MealId,
        MenuDate = request.Date,
        Portions = request.Portions  
    };

    db.MenuItems.Add(newMenuEntity);
    await db.SaveChangesAsync();

    var responseDto = new MenuDto
    {
        Id = newMenuEntity.MenuId,
        Date = newMenuEntity.MenuDate,
        Portions = newMenuEntity.Portions,
        MealId = newMenuEntity.MealId,
        MealName = meal.Name ?? "Unknown"
    };

    return TypedResults.Created($"/menus/{newMenuEntity.MenuId}", responseDto);
}

    public static async Task<IResult> UpdateMenu(int id, MenuRequestDto request, MealDbContext db)
    {
        var existingMenu = await db.MenuItems.FindAsync(id);
        if (existingMenu == null)
        {
            return TypedResults.NotFound();
        }

        var mealExists = await db.Meals.AnyAsync(m => m.MealId == request.MealId && m.IsActive);
        if (!mealExists)
        {
            return TypedResults.BadRequest("Zvolené jídlo neexistuje nebo není aktivní!");
        }

        existingMenu.MenuDate = request.Date;
        existingMenu.Portions = request.Portions;
        existingMenu.MealId = request.MealId;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteMenu(int id, MealDbContext db)
    {
        var existingMenu = await db.MenuItems.FindAsync(id);
        if (existingMenu == null)
        {
            return TypedResults.NotFound();
        }

        db.MenuItems.Remove(existingMenu);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> PrintOrders(MealDbContext db)
    {
        var orders = await db.Orders.Include(o => o.Menu)
                                        .ThenInclude(m => m!.Meal)
                                    .ToListAsync();

        var OrderDTOs = orders.Select(o => new OrderDto
        {
            Id = o.OrderId,
            MenuId = o.MenuId,
            Status = o.Status.ToString(),
            Date = o.Menu?.MenuDate,
            MealName = o.Menu?.Meal?.Name ?? "Unknown name"

        });

        return TypedResults.Ok(OrderDTOs);
    }

    public static async Task<IResult> CreateNewOrder(OrderRequestDto request, MealDbContext db)
{
    var menu = await db.MenuItems
        .Include(m => m.Meal)
        .FirstOrDefaultAsync(m => m.MenuId == request.MenuId);

    if (menu == null)
    {
        return TypedResults.NotFound("Zvolené menu neexistuje.");
    }

    if (menu.Portions <= 0)
    {
        return TypedResults.BadRequest("Omlouváme se, toto jídlo je již vyprodané!");
    }

    menu.Portions -= 1;

    var newOrder = new Order
    {
        MenuId = request.MenuId,
        Status = OrderStatus.Preparing
    };

    db.Orders.Add(newOrder);
    
    await db.SaveChangesAsync();

    var responseDto = new OrderDto
    {
        Id = newOrder.OrderId,
        MenuId = newOrder.MenuId,
        Status = newOrder.Status.ToString(),
        Date = menu.MenuDate,
        MealName = menu.Meal?.Name ?? "Unknown name"
    };

    return TypedResults.Created($"/orders/{newOrder.OrderId}", responseDto);
}

    public static async Task<IResult> UpdateOrderStatus(int id, OrderStatusRequestDto request, MealDbContext db)
    {
        var order = await db.Orders.Include(o => o.Menu).FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
        {
            return TypedResults.NotFound("Objednávka nebyla nalezena!");
        }

        if (request.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled){
            if (order.Menu != null)
            {
                order.Menu.Portions += 1;
            }
        }
        else if (order.Status == OrderStatus.Cancelled && request.Status != OrderStatus.Cancelled)
        {
            if (order.Menu != null)
            {
                order.Menu.Portions -= 1;
            }
        }

        order.Status = request.Status;
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}