namespace TrackerAPI.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }

        // Возвращаем не саму почту, а её часть или просто факт наличия
        public string? EmailMask { get; set; }
    }
}
