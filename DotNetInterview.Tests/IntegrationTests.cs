using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Json;
// using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API;
using DotNetInterview.API.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetInterview.Tests.B_Itegration
{
    [TestFixture]
    public class IntegrationTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;

        public IntegrationTests()
        {

        }

        [SetUp]
        public async Task Setup()
        {
            _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // services.AddDbContext<DataContext>(options =>
                    //     options.UseInMemoryDatabase("TestDatabase"));
                    // Here, you can directly configure the logging service.
                    services.AddLogging(builder =>
                    {


                        builder.AddConsole();  // Enable console logs
                        builder.SetMinimumLevel(LogLevel.Debug);  // Adjust log level if needed
                    });
                });
            });

            _client = _factory.CreateClient();
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
            var newItem = new Item { Name = "Item 1", Reference = "ITM1", Price = 40.00m };
            await CreateItem(newItem);

            // Act
            var response = await _client.GetAsync($"/items/{newItem.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains(newItem.Name)); // Ensure the content contains the name of the item
        }
        [Test]
        public async Task GetSingleItem_ReturnsVariationData_WhenItemExists()
        {
            // Arrange
            var newItem = new Item
            {
                Name = "Item 2",
                Reference = "ITM2",
                Price = 40.00m,
                Variations = new List<Variation> { new Variation{
                                Size = "10",
                                Quantity = 8
                }}

            };
            await CreateItem(newItem);


            // Act
            var response = await _client.GetAsync($"/items/{newItem.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("JSON:");
            Console.WriteLine(content);
            var contentItem = JsonConvert.DeserializeObject<Item>(content);
            Assert.IsTrue(contentItem.Variations.Any());
        }
        [Test]
        public async Task GetSingleItem_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/items/99999999-9999-9999-9999-999999999999");
            // 99999999-9999-9999-9999-999999999999 should not exist

            // Assert
            // Status Code 404
            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }
        [Test]
        public async Task GetSingleItem_ReturnsBadREquest_WhenIemWrongFormat()
        {
            // Act
            var response = await _client.GetAsync("/items/999");
            // 999 can't be parsed into a guid

            // Assert
            // Status Code 400
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
        }
        [Test]
        public async Task CreateItem_ReturnsCreated()
        {
            // Arrange
            var newItem = new Item { Name = "New Item", Reference = "New1", Price = 40.00m };

            var response = await _client.PostAsJsonAsync("/items", newItem);
            // Assert
            response.EnsureSuccessStatusCode();
            var createdItem = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(createdItem.Contains("New Item")); // Ensure the created item contains the name
            // Assert.That(response.IsSucessStatusCode, Is.True);
        }

        [Test]
        public async Task UpdateItem_ReturnsNoContent_WhenItemExists()
        {
            // Arrange
            var newItem = new Item { Name = "Old Item", Reference = "Old1", Price = 40.00m };
            await CreateItem(newItem);

            var updatedItem = new Item { Id = newItem.Id, Name = "Updated Item", Reference = "Upd1", Price = 40.00m };

            // Act
            var response = await _client.PutAsJsonAsync("/items", updatedItem);
            // Assert
            // Status Code 204 (No Content)
            Assert.That((int)response.StatusCode, Is.EqualTo(204));
            // Verify the update
            var getResponse = await _client.GetAsync($"/items/{updatedItem.Id}");
            var updatedContent = await getResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(updatedContent.Contains(updatedItem.Name));
        }

        [Test]
        public async Task DeleteItem_ReturnsNoContent_WhenItemExists()
        {
            // Arrange
            var newItem = new Item { Name = "Item to delete", Reference = "Del1", Price = 40.00m };
            await CreateItem(newItem);

            // Act
            var response = await _client.DeleteAsync($"/items/{newItem.Id}");

            // Assert
            Assert.That((int)response.StatusCode, Is.EqualTo(204));// Status Code 204 (No Content)
            // Verify the item is deleted
            var getResponse = await _client.GetAsync($"/items/{newItem.Id}");
            Assert.That((int)getResponse.StatusCode, Is.EqualTo(404)); // 404 Not Found
        }

        private async Task CreateItem(Item item)
        {


            var response = await _client.PostAsJsonAsync("/items", item);
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseItem = JsonConvert.DeserializeObject<Item>(responseBody);
            if (responseItem == null)
            {
                return;
            }
            item.Id = responseItem.Id;
        }
    }
}
