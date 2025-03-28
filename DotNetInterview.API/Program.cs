using DotNetInterview.API;
using DotNetInterview.API.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connection to database
var connection = new SqliteConnection("Data Source=DotNetInterview;Mode=Memory;Cache=Shared");
connection.Open();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connection));

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
app.MapGet("/items", async (DataContext context) =>
{
    return await context.Items.ToListAsync();
});
// Get a single item
app.MapGet("/items/{id}", async (DataContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item);
});
// Create a new item
app.MapPost("/items", async (DataContext context, Item newItem) =>
{
    await context.Items.AddAsync(newItem);
    context.SaveChanges();
});
// Update an item
app.MapPut("/items/", async (DataContext context, Item newItem) =>
{
    var id = newItem.Id;
    var oldItem = await context.Items.FindAsync(id);
    if (oldItem == null)
    {
        return Results.NotFound();
    }
    context.Items.Remove(oldItem);
    // replace
    await context.Items.AddAsync(newItem);
    context.SaveChanges();
    return Results.NoContent();
});
// Delete an item
app.MapDelete("/items/{id}", async (DataContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();
    }
    context.Items.Remove(item);
    context.SaveChanges();
    return Results.NoContent();
});



app.Run();
