using Microsoft.EntityFrameworkCore;
using TrackerAPI.Data.Entities;

namespace TrackerAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        // Ваши таблицы
        public DbSet<User> Users { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                // Если опции уже настроены (фабрикой дизайнера ИЛИ Program.cs) - не трогаем их.
                return;
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseInMemoryDatabase("WalkerLocalDB");
            }
            else
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Здесь будут настройки Fluent API для GeoJSON позже
        }
    }
}
