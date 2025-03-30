using DotNetInterview.API;
using DotNetInterview.API.Domain;
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
var connection = new SqliteConnection("Data Source=DotNetInterview;Mode=Memory;Cache=Shared");
connection.Open();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connection));
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
    var items = itemService.GetAllItems();
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
    
    Item item =  await itemService.PostItem(newItem);
    return Results.Ok(item);
});
// Update an item
app.MapPut("/items/", async (ItemService itemService, Item newItem) =>
{
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
