# 🍴 Objednávací systém v menze (UTB Minute)

#### Semestrální projekt do předmětu **Aplikační frameworky**.
##### Půlsemestrální odevzdání

Cílem projektu je návrh a implementace objednávacího systému pro menzu s využitím nástrojů a frameworků .NET Aspire, Minimal WebAPI, Entity Framework Core a Blazor.
#####
Objednávací systém pro menzu umožňuje objednávání minutek (jídel připravovaných na objednávku). Student si objedná jídlo ve webové aplikaci běžící na dotykovém panelu. Kuchařky následně jídlo připravují a mění stav objednávky ve své webové aplikaci. Student je o stavu objednávky informován v reálném čase.


## 👥 Členové týmu a poměr práce
| Jméno a příjmení            | Role v týmu                 | Poměr práce |
| :-------------------------- | :-------------------------- | :---------: |
| **Dorien Herman** - vedoucí | Database & DbContext        |      1      |
| **Jakub Prusenovský**       | WebAPI & WebAPI testy & DTO |      1      |
| **Iva Trochtová**           | Frontend & Klienti          |      (1)    |
| **Tomáš Přikryl**           | Keycloak & Zabezpečení      |      (1)    |

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
- **Keycloak** — připraveno pro autentizaci (zatím nenasazeno)
- **xUnit** — unit a integrační testy


---

## Struktura řešení

- `AspireApp1.ServiceDefaults`: Sdílená konfigurace služeb (health checks, telemetrie)
- `UTB.Minute.AppHost`: Aspire orchestrace - definuje kontejnery a jejich propojení
- `UTB.Minute.Db`: Databáze a DbContext
- `UTB.Minute.DbManager`: Obsahuje reset Databáze
- `UTB.Minute.Contracts`: Sdílené DTO (Data Transfer Objects) pro WebApi
- `UTB.Minute.WebApi`: Správa objednávek, jídel a meníček. Obsahuje endpointy pro **Http Commandy** 
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
- [x] Http Command pro reset databáze
- [x] Seed testovacích dat funguje
- [x] Service Discovery bez pevných adres

---

### Testy a dokumentace
- [x] Stručná dokumentace projektu (README.md)

---

## 📝 Poznámky k odevzdání
* **Stav:** Projekt je spustitelný, ale nemá UI ani zabezpečení.
* **Testování:** Unit testy v `UTB.Minute.WebApi.Tests` pokrývají scénář od vytvoření jídla až po jeho výdej.
* **Problémy:** Při tvorbě projektu nenastaly žádné zásadní komplikace.