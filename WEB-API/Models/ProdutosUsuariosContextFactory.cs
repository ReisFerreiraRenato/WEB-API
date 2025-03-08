using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WEB_API.Models
{
    public class ProdutosUsuariosContextFactory : IDesignTimeDbContextFactory<ProdutosUsuariosContext>
    {
        public ProdutosUsuariosContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("ProdutosUsuariosDB");

            var optionsBuilder = new DbContextOptionsBuilder<ProdutosUsuariosContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ProdutosUsuariosContext(optionsBuilder.Options);
        }
    }
}