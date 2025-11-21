namespace Craftmatrix.org.DB
{
    public class LoginLogsDto
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Logged { get; set; }
    }
}
