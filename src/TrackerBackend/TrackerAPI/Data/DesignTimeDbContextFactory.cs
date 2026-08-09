using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TrackerAPI.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // ЖЕСТКО указываем временную строку для инструмента миграций.
            // Неважно, существует ли эта БД на диске, инструменту нужна только строка,
            // чтобы понять, что использовать именно Npgsql.
            var connectionString = "Host=localhost;Database=Walker_DesignTime_Temp;Username=postgres;Password=password";

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options, null!);
        }
    }
}
