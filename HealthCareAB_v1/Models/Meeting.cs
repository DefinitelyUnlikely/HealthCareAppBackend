using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Models
{
    public class Meeting
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        public DateTime? ExpiresAt { get; set; }
        [Required]
        public bool Canceled { get; set; } = false;
        [Required]
        [Column(TypeName = "text")]
        public MeetingStatus Status { get; set; }
        public string? Notes { get; set; }
        [Required]
        public int PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public User? Patient { get; set; }
        [Required]
        public int CaregiverId { get; set; }
        [ForeignKey(nameof(CaregiverId))]
        public User? Caregiver { get; set; }
    }

    public enum MeetingStatus
    {
        Pending,
        Confirmed,
        Unavailable
    }

}

