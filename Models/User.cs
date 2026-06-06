namespace VulnerableApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }

        // Mantén la original para no romper la base de datos ni el AppDbContext
        public string? Password { get; set; }

        // Agrega la nueva para tu código de remediación
        public string? PasswordHash { get; set; }

        public string? Email { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}