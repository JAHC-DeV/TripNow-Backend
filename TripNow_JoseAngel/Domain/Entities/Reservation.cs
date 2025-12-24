using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TripNow_JoseAngel.Domain.Enums;

namespace TripNow_JoseAngel.Domain.Entities
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [EmailAddress] 
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string TripCountry { get; set; } = string.Empty;

        [Required]
        public int Amount { get; set; }

        [Required]
        [DefaultValue(ReservationStatus.PENDING_RISK_CHECK)]
        public ReservationStatus Status { get; set; } = ReservationStatus.PENDING_RISK_CHECK;

        [Required]        
        public double? RiskScore { get; set; } = null;

        [Required]
        public string IdempotencyKey { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
