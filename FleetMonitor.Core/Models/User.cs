namespace FleetMonitor.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public string Role { get; set; } = "user"; 
    }
}
