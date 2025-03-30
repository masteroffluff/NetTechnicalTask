using DotNetInterview.API;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Infrastructure;
using DotNetInterview.API.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("DotNetInterview.Tests")] // give access to the integration tests

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connection to database
// Moved this to its own layer to ease rpacement, also added strings to the config file 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=DotNetInterview;Mode=Memory;Cache=Shared";
builder.Services.AddDataAccess(connectionString);
builder.Services.AddScoped<ItemService>();

var app = builder.Build();


// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// add exception handling
// Global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

// add logging to environment
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

app.MapGroup("/items")
    .WithTags("Items");

// List all items
app.MapGet("/items", async (ItemService itemService) =>
{
    var items = await itemService.GetAllItems();
    return items;
});

// Get a single item
app.MapGet("/items/{id}", async (ItemService itemService, string id) =>
{
    // validate the id 
    if (!Guid.TryParse(id, out Guid validId))
    {
        return Results.BadRequest("Invalid ID format.");
    }
    // get the item from the item service
    var item = await itemService.GetSingleItem(validId);

    if (item == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item);
});
// Create a new item
app.MapPost("/items/", async (ItemService itemService, Item newItem) =>
{
    // validate inbound data
    if (
        newItem.Name == "" ||
        newItem.Reference == "" ||
        newItem.Price == 0.00m
    )
    {
        return Results.BadRequest("Invalid Item Data.");
    }
    Item item = await itemService.PostItem(newItem);
    return Results.Ok(item);
});
// Update an item
app.MapPut("/items/", async (ItemService itemService, Item newItem) =>
{
    // validate inbound data (no need to validate the Id as this happens on the next step)
    if (
        newItem.Name == "" ||
        newItem.Reference == "" ||
        newItem.Price == 0.00m
    )
    {
        return Results.BadRequest("Invalid Item Data.");
    }
    bool itemFoundAndUpdated = await itemService.UpdateItem(newItem);
    if (!itemFoundAndUpdated)
    {
        return Results.NotFound();
    }
    return Results.NoContent();
});
// Delete an item
app.MapDelete("/items/{id}", async (ItemService itemService, string id) =>
{
    // validate the incoming id 
    if (!Guid.TryParse(id, out Guid validId))
    {
        return Results.BadRequest("Invalid ID format.");
    }
    // if id good delete the item 
    bool itemFoundAndDeleted = await itemService.DeleteItem(validId);
    if (!itemFoundAndDeleted)
    {
        return Results.NotFound();
    }
    return Results.NoContent();
});



app.Run();
