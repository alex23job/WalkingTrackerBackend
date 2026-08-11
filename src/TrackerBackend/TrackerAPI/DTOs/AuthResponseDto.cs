using Newtonsoft.Json;

namespace TrackerAPI.DTOs
{
    public class AuthResponseDto
    {
        [JsonProperty("token")]
        public string Token { get; set; } = null!;
    }
}
