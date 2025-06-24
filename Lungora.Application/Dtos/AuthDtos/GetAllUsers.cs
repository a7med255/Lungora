namespace Lungora.Dtos.AuthDtos
{
    public class GetAllUsers
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ImageUser { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public List<string>Roles { get; set; } = new();
    }
}
