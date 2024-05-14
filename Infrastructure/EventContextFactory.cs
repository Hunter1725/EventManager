using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class TodoContextFactory : IDesignTimeDbContextFactory<EventContext>
    {
        public EventContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory().ToString())
            .AddJsonFile("setting.json", optional: true, reloadOnChange: true)
            .Build();

            // Get connection string from configuration
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<EventContext>();
            optionsBuilder.UseSqlServer(connectionString);


            return new EventContext(optionsBuilder.Options);
        }
    }
}
