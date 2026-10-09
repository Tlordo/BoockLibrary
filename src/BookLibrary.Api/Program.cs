var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Фракции из README библиотеки Warhammer 40,000
var factions = new List<Faction>
{
    new(1, "Империум Человечества", "Человечество под властью Императора"),
    new(2, "Космический Десант", "Генетически улучшенные воины Императора"),
    new(3, "Эльдары", "Древняя раса псайкеров"),
    new(4, "Тёмные Эльдары", "Друхари — садисты из Комморры"),
    new(5, "Орки", "ВАААГХ! Зелёная раса, созданная Кроками"),
    new(6, "Тираниды", "Космический рой, пожирающий миры"),
    new(7, "Некроны", "Древние металлические повелители"),
    new(8, "Тау", "Молодая империя Высшего Блага"),
    new(9, "Хаос", "Силы Варпа и демоны"),
    new(10, "Генокрады", "Культы, заражённые тиранидами"),
};

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "BookLibrary.Api" }));

app.MapGet("/factions", () => factions);

app.MapGet("/factions/{id:int}", (int id) =>
    factions.FirstOrDefault(f => f.Id == id) is { } f ? Results.Ok(f) : Results.NotFound());

app.Run();

record Faction(int Id, string Name, string Description);
