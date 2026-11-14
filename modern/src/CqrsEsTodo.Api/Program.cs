using CqrsEsTodo.Api;
using CqrsEsTodo.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<CqrsTodoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TodoDb")));
builder.Services.AddScoped<TodoApplicationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<CqrsTodoDbContext>();
    var hasPendingMigrations = (await database.Database.GetPendingMigrationsAsync()).Any();
    await database.Database.MigrateAsync();

    if (hasPendingMigrations && await database.TodoEvents.AnyAsync())
    {
        await scope.ServiceProvider.GetRequiredService<TodoApplicationService>()
            .RebuildReadModelAsync(CancellationToken.None);
    }
}

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async Task<IResult> (CqrsTodoDbContext database, CancellationToken cancellationToken) =>
    await database.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapGet("/api/lists", async (TodoApplicationService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetListsAsync(cancellationToken)));

app.MapGet("/api/lists/{id:guid}", async Task<IResult> (Guid id, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    var list = await service.GetListAsync(id, cancellationToken);
    return list is null ? Results.NotFound() : Results.Ok(list);
});

app.MapGet("/api/items/{id:guid}", async Task<IResult> (Guid id, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    var item = await service.GetItemAsync(id, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/lists", async Task<IResult> (CreateListRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = await service.CreateListAsync(request.Title, request.Description, cancellationToken);
        return Results.Created($"/api/lists/{list.Id}", list);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["title"] = [exception.Message] });
    }
});

app.MapPut("/api/lists/{id:guid}/description", async Task<IResult> (Guid id, ChangeDescriptionRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = await service.ChangeDescriptionAsync(id, request.Description, cancellationToken);
        return list is null ? Results.NotFound() : Results.Ok(list);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["description"] = [exception.Message] });
    }
});

app.MapPost("/api/lists/{id:guid}/items", async Task<IResult> (Guid id, AddItemRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = await service.AddItemAsync(id, request.Description, request.DueAt, request.Importance, cancellationToken);
        return list is null ? Results.NotFound() : Results.Created($"/api/lists/{id}", list);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["description"] = [exception.Message] });
    }
});

app.MapPut("/api/lists/{listId:guid}/items/{itemId:guid}/completion", async Task<IResult> (Guid listId, Guid itemId, CompleteItemRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = request.Completed
            ? await service.CompleteItemAsync(listId, itemId, cancellationToken)
            : await service.ReopenItemAsync(listId, itemId, cancellationToken);
        return list is null ? Results.NotFound() : Results.Ok(list);
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapPut("/api/lists/{listId:guid}/items/{itemId:guid}/importance", async Task<IResult> (Guid listId, Guid itemId, ChangeImportanceRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = await service.ChangeItemImportanceAsync(listId, itemId, request.Importance, cancellationToken);
        return list is null ? Results.NotFound() : Results.Ok(list);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapPut("/api/lists/{listId:guid}/items/{itemId:guid}/due-date", async Task<IResult> (Guid listId, Guid itemId, ChangeDueDateRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = await service.ChangeItemDueDateAsync(listId, itemId, request.DueAt, cancellationToken);
        return list is null ? Results.NotFound() : Results.Ok(list);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapPut("/api/lists/{listId:guid}/items/{itemId:guid}/description", async Task<IResult> (Guid listId, Guid itemId, ChangeItemDescriptionRequest request, TodoApplicationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var list = await service.ChangeItemDescriptionAsync(listId, itemId, request.Description, cancellationToken);
        return list is null ? Results.NotFound() : Results.Ok(list);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["description"] = [exception.Message] });
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/maintenance/rebuild-read-model", async (TodoApplicationService service, CancellationToken cancellationToken) =>
    {
        await service.RebuildReadModelAsync(cancellationToken);
        return Results.NoContent();
    });
}

app.Run();
