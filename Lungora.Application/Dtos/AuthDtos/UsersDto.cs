namespace Lungora.Dtos.AuthDtos
{
    public class UsersDto
    {
        public string? Name { get; set; }
        public string? ImageUser { get; set; }
        public string? Email { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? Role { get; set; }
    }
}
