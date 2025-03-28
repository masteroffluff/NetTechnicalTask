using NUnit.Framework;
// using System.Net.Http;
using System.Text;
// using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API;
using DotNetInterview.API.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApiTests
{

    public class ApiTests
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ApiTests()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // builder.ConfigureServices(services =>
                    // {
                       
                    //     // Here, you can directly configure the logging service.
                    //     services.AddLogging(builder =>
                    //     {
                            
                    //         builder.AddConsole();  // Enable console logs
                    //         builder.SetMinimumLevel(LogLevel.Debug);  // Adjust log level if needed
                    //     });
                    // });
                });
            _client = _factory.CreateClient();
        }

        [SetUp]
        public async Task Setup()
        {
            // Reset the database before each test
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            await context.Database.EnsureDeletedAsync();  // Ensure the database is deleted before each test
            await context.Database.EnsureCreatedAsync();  // Ensure the database is created before each test
        }

        [Test]
        public async Task GetAllItems_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/items");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsNotEmpty(content);
        }

        [Test]
        public async Task GetSingleItem_ReturnsOk_WhenItemExists()
        {
            // Arrange
            var newItem = new Item { Name = "Item 1" };
            await CreateItem(newItem);

            // Act
            var response = await _client.GetAsync($"/items/{newItem.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains(newItem.Name)); // Ensure the content contains the name of the item
        }

        [Test]
        public async Task GetSingleItem_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/items/999"); // 999 should not exist

            // Assert
            Assert.AreEqual(404, (int)response.StatusCode); // Status Code 404
        }

        [Test]
        public async Task CreateItem_ReturnsCreated()
        {
            // Arrange
            var newItem = new Item { Name = "New Item" };
            var json = $"{{\"name\": \"{newItem.Name}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/items", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdItem = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(createdItem.Contains(newItem.Name)); // Ensure the created item contains the name
        }

        [Test]
        public async Task UpdateItem_ReturnsNoContent_WhenItemExists()
        {
            // Arrange
            var newItem = new Item { Name = "Old Item" };
            await CreateItem(newItem);

            var updatedItem = new Item { Id = newItem.Id, Name = "Updated Item" };
            var json = $"{{\"id\": {updatedItem.Id}, \"name\": \"{updatedItem.Name}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/items", content);

            // Assert
            Assert.AreEqual(204, (int)response.StatusCode); // Status Code 204 (No Content)

            // Verify the update
            var getResponse = await _client.GetAsync($"/items/{updatedItem.Id}");
            var updatedContent = await getResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(updatedContent.Contains(updatedItem.Name));
        }

        [Test]
        public async Task DeleteItem_ReturnsNoContent_WhenItemExists()
        {
            // Arrange
            var newItem = new Item { Name = "Item to delete" };
            await CreateItem(newItem);

            // Act
            var response = await _client.DeleteAsync($"/items/{newItem.Id}");

            // Assert
            Assert.That((int)response.StatusCode, Is.EqualTo(204));// Status Code 204 (No Content)
            // Verify the item is deleted
            var getResponse = await _client.GetAsync($"/items/{newItem.Id}");
            Assert.AreEqual(404, (int)getResponse.StatusCode); // 404 Not Found
        }

        private async Task CreateItem(Item item)
        {
            var json = $"{{\"name\": \"{item.Name}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PostAsync("/items", content);
        }
    }
}
