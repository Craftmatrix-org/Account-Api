namespace Craftmatrix.org.Dto
{
    public class Whoami
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }

    }
}
