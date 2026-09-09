using System.ComponentModel.DataAnnotations;

namespace KMC.Api.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash {  get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
