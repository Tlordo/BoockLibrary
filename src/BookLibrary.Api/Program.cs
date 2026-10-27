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
var sync = new object(); // List<T> не потокобезопасен, а запросы идут параллельно

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "BookLibrary.Api" }));

// GET — все фракции
app.MapGet("/factions", () =>
{
    lock (sync) return Results.Ok(factions.ToList());
});

// GET — одна фракция
app.MapGet("/factions/{id:int}", (int id) =>
{
    lock (sync)
        return factions.FirstOrDefault(f => f.Id == id) is { } f ? Results.Ok(f) : Results.NotFound();
});

// POST — создать
app.MapPost("/factions", (FactionRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = "Name обязателен" });

    lock (sync)
    {
        var id = factions.Count == 0 ? 1 : factions.Max(f => f.Id) + 1;
        var faction = new Faction(id, req.Name, req.Description ?? "");
        factions.Add(faction);
        return Results.Created($"/factions/{id}", faction);
    }
});

// PUT — обновить целиком
app.MapPut("/factions/{id:int}", (int id, FactionRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = "Name обязателен" });

    lock (sync)
    {
        var index = factions.FindIndex(f => f.Id == id);
        if (index < 0) return Results.NotFound();

        var updated = factions[index] with { Name = req.Name, Description = req.Description ?? "" };
        factions[index] = updated;
        return Results.Ok(updated);
    }
});

// DELETE — удалить
app.MapDelete("/factions/{id:int}", (int id) =>
{
    lock (sync)
        return factions.RemoveAll(f => f.Id == id) > 0 ? Results.NoContent() : Results.NotFound();
});

app.Run();

record Faction(int Id, string Name, string Description);
record FactionRequest(string Name, string? Description);
