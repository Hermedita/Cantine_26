# 🍴 Objednávací systém v menze (UTB Minute)

#### Semestrální projekt do předmětu **Aplikační frameworky**.
##### Semestrální/Finální odevzdání

Cílem projektu je návrh a implementace objednávacího systému pro menzu s využitím nástrojů a frameworků .NET Aspire, Minimal WebAPI, Entity Framework Core a Blazor.
#####
Objednávací systém pro menzu umožňuje objednávání minutek (jídel připravovaných na objednávku). Student si objedná jídlo ve webové aplikaci běžící na dotykovém panelu. Kuchařky následně jídlo připravují a mění stav objednávky ve své webové aplikaci. Student je o stavu objednávky informován v reálném čase.


## 👥 Členové týmu a poměr práce
| Jméno a příjmení            | Role v týmu                 | Poměr práce |
| :-------------------------- | :-------------------------- | :---------: |
| **Dorien Herman** - vedoucí | Database & DbContext        |      1      |
| **Jakub Prusenovský**       | WebAPI & WebAPI testy & DTO |      1      |
| **Iva Trochtová**           | Frontend & Klienti          |      1      |
| **Tomáš Přikryl**           | Keycloak & SSE              |      1      |

---

## Spuštění projektu

1. **Požadavky:** .NET 10 SDK, Docker Desktop nebo Podman (nutný pro běh SQL Serveru a Keycloaku v Aspire).
2. **Postup:**
   - Spusťe Docker Desktop nebo Podman.
   - Otevřete solution `UTB.Minute.slnx` ve Visual Studiu 2026 nebo JetBrains Rider.
   - Nastavte projekt `UTB.Minute.AppHost` jako **Start-up projekt**.
   - Spusťte projekt.
   - V prohlížeči se otevře **.NET Aspire Dashboard**, kde uvidíte stav všech služeb a odkazy na klientské aplikace.

---
## Technologie

- **.NET 10** — backend, WebAPI, testy
- **ASP.NET Core Minimal API** — HTTP endpointy
- **.NET Aspire** — orchestrace služeb a service discovery
- **Entity Framework Core** — přístup k databázi
- **SQLite Server** — relační databáze (běží v kontejneru)
- **Keycloak** — pro autentizaci/zabezpečení
- **xUnit** — unit a integrační testy
- **Server-Sent Events (SSE)** — serverem iniciované notifikace
- **Service Discovery** — nemá pevně zadané lokální adresy


---

## Struktura řešení

- `AspireApp1.ServiceDefaults`: Sdílená konfigurace služeb (health checks, telemetrie)
- `UTB.Minute.AdminClient`: Blazor Server aplikace pro vedení menzy. Volá WebAPI pomocí protokolu HTTP.
- `UTB.Minute.AppHost`: Aspire orchestrace - definuje kontejnery a jejich propojení + SSE
- `UTB.Minute.CanteenClient`: Blazor Server aplikace pro uživatele menzy. Volá WebAPI pomocí protokolu HTTP.
- `UTB.Minute.Contracts`: Sdílené DTO (Data Transfer Objects) pro WebApi a klienty
- `UTB.Minute.Db`: Databáze a DbContext
- `UTB.Minute.DbManager`: Obsahuje reset Databáze
- `UTB.Minute.WebApi`: Správa objednávek, jídel a meníček. Obsahuje endpointy pro **Http Commandy** a SSE
- `UTB.Minute.WebApi.Tests`: Testování CRUD commandů v databázi a propojení s WebApi

#####

**Dokumenty se navzájem referencují.**

---

## Datový model a DTO

- [x] Entity a jejich vazby odpovídají zadání
- [x] Správně navržený DbContext
- [x] Stav objednávky řešen enumem
- [x] DTO jsou definována pouze v UTB.Minute.Contracts
- [x] WebAPI nevrací entity přímo

### Přehled Db objektů (UTB.Minute.Db)
| Objekt  | Popis                                                                     |
|---------|---------------------------------------------------------------------------|
| `Meal`  | Jídlo (`MealId`, `Name`, `Description`, `Price`,  `IsActive`)             |
| `Menu`  | Menu s jídly (`MenuId`, `MealId`, `MenuDate`, `Portions`, ref. na `Meal`) |
| `Order` | Objednávka (`OrderId`, `MenuId`, `Status`, ref. na `Menu`)                |


### Přehled DTO Entit (UTB.Minute.Contracts)
| Entita                  | Popis                                                        |
|-------------------------|--------------------------------------------------------------|
| `MealDto`               | Jídlo (`Id`, `Name`, `Price`, `Description`, `IsActive`)     |
| `MealRequestDto`        | Jídlo (`Name`, `Price`, `Description`)                       |
| `MealStateRequestDto`   | Jídlo (`IsActive`)                                           |
| `MenuDto`               | Menu s jídly (`Id`, `Date`, `Portions`, `MealId`,`MealName`) |
| `MenuRequestDto`        | Menu s jídly (`Date`, `Portions`, `MealId`)                  |
| `OrderDto`              | Objednávka (`Id`,`Status`,`MenuId``Date`,`MealName`)         |
| `OrderRequestDto`       | Objednávka (`MenuId`)                                        |
| `OrderStatusRequestDto` | Objednávka (`Status`)                                        |

### Stav objednávky (`OrderStatus`)

```csharp
public enum OrderStatus
{
    Preparing, //lower number of items available
    Ready, //ready to take
    Cancelled, //order does not return number of items
    Finished
}
```

---
## Funkčnost WebAPI a jeho testy

### Jídla
- [x] Vytvoření a čtení jídel a jejich testy
- [x] Úprava jídla + deaktivace a jejich testy

#### Seznam API endpointů (Meals)

|   |                                      |                                       |   |
|---|--------------------------------------|---------------------------------------|---|
|   |`GET {{HostAddress}}/meals`           | Seznam všech jídel                    |   |
|   |`POST {{HostAddress}}/meals`          | Vytvoření nového jídla                |   |
|   |`PUT {{HostAddress}}/meals/1`         | Změna jídla                           |   |
|   |`PATCH  {{HostAddress}}/meals/1/state`| Změní stav jídla, valid -> true/false |   |
|   |                                      |                                       |   |

#### Příklad API endpointu
##### např. `POST {{HostAddress}}/meals`
```json
POST {{HostAddress}}/meals
Content-Type: application/json

{
  "name": "Rajská",
  "price": 99,
  "description": "S knedlíky!"
}
###
```

### Menu
- [x] Vytvoření a čtení položek menu a jejich testy
- [x] Úprava a smazání položek menu a jejich testy

#### Seznam API endpointů (Menus)

|  |                                |                       |   |
|--|--------------------------------|-----------------------|---|
|  |`GET {{HostAddress}}/menus`     | Seznam všech meníček  |   |
|  |`POST {{HostAddress}}/menus`    | Vytvoření nového menu |   |
|  |`PUT {{HostAddress}}/menus/1`   | Změna menu            |   |
|  |`DELETE {{HostAddress}}/menus/1`| Vymazat menu          |   |
|  |                                |                       |   |

#### Příklad API endpointu
##### např. `PUT {{HostAddress}}/menus/1`
```json
PUT {{HostAddress}}/menus/1
Content-Type: application/json

{
    "date": "2026-04-05",
    "portions": 67,
    "mealId": 1
}

###
```

### Objednávky
- [x] Vytvoření a čtení objednávek a jejich testy
- [x] Změna stavu objednávky a jeho test

#### Seznam API endpointů (Orders)

|  |                                |                         |  |
|--|--------------------------------|-------------------------|--|
|  |`GET {{HostAddress}}/orders`  | Seznam všech objednávek   |  |
|  |`POST {{HostAddress}}/orders` | Vytvoření nové objednávky |  |
|  |                                |                         |  |

#### Příklad API endpointu
##### např. `POST {{HostAddress}}/orders`
```json
POST {{HostAddress}}/orders
Content-Type: application/json

{
  "menuId": 2
}
```

---

## Aspire integrace
- [x] Databáze vytvořena a konfigurována přes Aspire
```csharp
<PackageReference Include="Aspire.Microsoft.EntityFrameworkCore.SqlServer" Version="13.1.1" />
```
- [x] Http Command pro reset databáze
```csharp
app.MapPost("/reset-db", async (MealDbContext context) =>  //changed from DbContext to MealDbContext
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
    await context.SaveChangesAsync();
});
```
- [x] Seed testovacích dat funguje
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MealDbContext>();

    await context.Database.EnsureCreatedAsync();

    if (!context.Meals.Any())
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly tomorrow = today.AddDays(1);

        var rizek = new Meal { Name = "Kuřecí řízek", Price = 135, IsActive = true, Description = "Smažený řízek" };
        var smazak = new Meal { Name = "Smažák", Price = 120, IsActive = true, Description = "Sýr" };

        var rizekMenu = new Menu { Meal = rizek, MenuDate = today, Portions = 50 };
        var smazakMenu = new Menu { Meal = smazak, MenuDate = tomorrow, Portions = 0 };

        context.Meals.AddRange(rizek, smazak);
        context.MenuItems.AddRange(rizekMenu, smazakMenu);

        await context.SaveChangesAsync();
    }
}
```
- [x] Service Discovery bez pevných adres

---

## Projekty a integrace
- [x] `AdminClient` a `CanteenClient` napojené na WebAPI
```csharp
public class CanteenService(HttpClient httpClient)
{
    public async Task<MenuDto[]?> GetMenusAsync()
    {
        return await httpClient.GetFromJsonAsync<MenuDto[]>("/menus");
    }
...
```
- [x] Backend plně funkční a použitý oběma klienty
```csharp
using UTB.Minute.Contracts;
```
---

## Funkcionalita klienta
*Přihlašovací údaje jsou v "UTB.Minute.AppHost/import/utb-minute-users-0.json"*

### Student (Karel)
- [x] Zobrazení menu pro aktuální den
- [x] Zobrazení seznamu objednávek
- [x] Objednání jídla + snížení počtu porcí
- [x] Vyprodaná jídla vizuálně odlišena (šedě + readonly)

---

### Kuchařka (Kucharka)
- [x] Zobrazení nedokončených objednávek
- [x] Změna stavu objednávky (hotová / zrušená / dokončená)
- [x] Neplatné přechody objednávek jsou blokovány (např. nelze přejít ze 'Zrušeno' na 'Hotovo')

---

### Jídla
- [x] Vytváření jídel
```csharp
  public async Task CreateMealAsync(MealRequestDto meal)
```
- [x] Úprava jídel
```csharp
public async Task UpdateMealAsync(MealRequestDto meal, int id)
```
- [x] Deaktivace jídla
```csharp
public async Task ChangeMealStateAsync(MealStateRequestDto mealStateRequest, int id)
```

### Menu
- [x] Vytváření položek menu
```csharp
public async Task CreateMenuAsync(MenuRequestDto menu)
```
- [x] Úprava položek menu
```csharp
public async Task UpdateMenuAsync(MenuRequestDto menu, int id)
```

---

### SSE notifikace
- [x] Funkční SSE endpoint
- [x] Notifikace pro studenta i kuchařku
```csharp
public async Task BroadcastOrderUpdateAsync(OrderUpdateMessage message)
```
- [x] Automatická aktualizace UI

---

### Autentizace a autorizace
- [x] Keycloak spuštěn přes Aspire
```csharp
<PackageReference Include="Aspire.Hosting.Keycloak" Version="13.3.5-preview.1.26270.6" />
```
- [x] Backend zabezpečen podle rolí
- [x] UI reaguje na roli uživatele

---

### Testy a dokumentace
- [x] Stručná dokumentace projektu (README.md)
- [x] Aktualizovaná dokumentace k finálnímu řešení

---

## 📝 Poznámky k odevzdání
* **Stav:** Projekt je spustitelný, a dokončený.
* **Testování:** Unit testy v `UTB.Minute.WebApi.Tests` pokrývají scénář od vytvoření jídla až po jeho výdej.
* **Problémy:** Při tvorbě projektu nenastaly žádné zásadní komplikace.
