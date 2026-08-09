using System.ComponentModel.DataAnnotations;

namespace TrackerAPI.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Заполните Email")]
        public string EmailHash { get; set; } // Хеш почты для приватности

        [Required(ErrorMessage = "Введите пароль")]
        public string PasswordHash { get; set; }

        public double StepLengthMeters { get; set; } = 0.72; // Дефолтная длина шага

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
