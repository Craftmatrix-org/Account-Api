namespace Craftmatrix.org.DB
{
    public class Whoami
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
