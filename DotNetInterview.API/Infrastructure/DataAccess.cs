using Microsoft.Data.Sqlite;
using DotNetInterview.API.Domain;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API;
namespace DotNetInterview.API.Infrastructure{
    public static class DataAccessExtensions
    {
        public static void AddDataAccess(this IServiceCollection services, string connectionString)
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            
            services.AddDbContext<DataContext>(options => options.UseSqlite(connection));
            services.AddSingleton(connection); 
        }
    }
}