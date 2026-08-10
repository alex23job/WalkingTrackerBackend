using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TrackerAPI.Data;
using TrackerAPI.Data.Entities;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace TrackerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------------------------------------------------------------
            // 1. НАСТРОЙКА БАЗЫ ДАННЫХ (EF Core + PostgreSQL / InMemory)
            // ------------------------------------------------------------
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                /*if (string.IsNullOrEmpty(connectionString))
                {
                    // Локальная разработка: база в оперативной памяти
                    options.UseInMemoryDatabase("WalkerLocalDB");
                }
                else
                {
                    // Production (Railway): реальный PostgreSQL
                    options.UseNpgsql(connectionString);
                }*/
                options.UseNpgsql(connectionString!);
            });

            // ------------------------------------------------------------
            // 2. СЛУЖБЫ (КОНТРОЛЛЕРЫ И API ИССЛЕДОВАТЕЛЬ)
            // ------------------------------------------------------------
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Отключаем принудительное изменение регистра
                                                                           // ИЛИ явно разрешаем маленькие буквы, если включено что-то другое:
                                                                           // options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; 
            });            // Add services to the container.

            // Регистрируем генератор OpenAPI
            builder.Services.AddEndpointsApiExplorer();

            // Регистрируем сам Swagger (Swashbuckle)
            builder.Services.AddSwaggerGen(options =>
            {
                // Добавляем кнопку Authorize для JWT прямо в UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введите JWT-токен вида: Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference // Теперь видит тип
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 3. АУТЕНТИФИКАЦИЯ И АВТОРИЗАЦИЯ (ДОБАВЬТЕ ЭТО)
            // Читаем секцию "Jwt" из конфигурации и проверяем её наличие
            var jwtSection = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JwtOptions>(jwtSection);

            // Проверка при старте: если ключей нет - приложение не запустится
            var jwtSettings = jwtSection.Get<JwtOptions>();
            if (jwtSettings == null ||
                string.IsNullOrWhiteSpace(jwtSettings.Key) ||
                string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            {
                throw new InvalidOperationException("Критическая ошибка: Настройки JWT не сконфигурированы.");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

            builder.Services.AddAuthorization();

            // ------------------------------------------------------------
            // 4. ПОСТРОЕНИЕ КОНТЕЙНЕРА ЗАПРОСОВ
            // ------------------------------------------------------------
            var app = builder.Build();

            // ------------------------------------------------------------
            // 5. КОНФИГУРАЦИЯ HTTP-КАНАЛА (MIDDLEWARE PIPELINE)
            // ------------------------------------------------------------

            // ВАЖНО: Порядок middleware имеет значение!
            app.UseHttpsRedirection();

            // Сначала идет аутентификация (проверка токена), потом авторизация (проверка ролей/прав)
            app.UseAuthentication();
            app.UseAuthorization();

            // Карта эндпоинтов контроллеров ([ApiController] атрибуты)
            app.MapControllers();

            // Включаем middleware генерации JSON спецификации Swagger
            app.UseSwagger();

            // Включаем интерактивный интерфейс Swagger UI
            // Он будет доступен по адресу /swagger
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Walker API V1");
                options.RoutePrefix = "swagger"; // URL страницы документации
            });
            app.Run();
        }
    }

    // Класс настроек для удобства (можно вынести в отдельный файл)
    public class JwtOptions
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
    }
}
