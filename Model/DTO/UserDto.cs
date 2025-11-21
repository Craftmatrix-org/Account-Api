namespace Craftmatrix.org.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public DateTime Joined { get; set; }

        public Whoami Whoami { get; set; }
    }
}
