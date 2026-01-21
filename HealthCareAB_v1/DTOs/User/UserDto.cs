namespace HealthCareAB_v1.DTOs
{
    public class UserDto
    {
        public required string Username { get; set; }
        public required List<string> Roles { get; set; }
    }
}

