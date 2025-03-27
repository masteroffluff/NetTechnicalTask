using DotNetInterview.API;
using DotNetInterview.API.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.

builder.Services.AddControllers();
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
app.MapGet("/items/all", (DataContext context) => "Get all items");
// Get a single item
app.MapGet("/items/{id}", (DataContext context, int id) => $"You requested item with ID: {id}");
// Create a new item
app.MapPost("/items", (DataContext context, Item newItem) => 
{
    return Results.Ok("This is a POST request");
});
// Update an item
app.MapPut("/items/{id}", (DataContext context, int id) => $"You updated item with ID: {id}");
// Delete an item
app.MapDelete("/items/{id}", (DataContext context, int id) => $"This is a DELETE request for {id}");

app.MapControllers();

app.Run();
