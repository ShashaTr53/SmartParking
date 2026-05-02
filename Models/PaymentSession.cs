using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Models
{
    public class PaymentSession
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // ✅ Foreign Key
        public int SpotId { get; set; }

        // ✅ Navigation Property
        public Spot? Spot { get; set; }

        public DateTime StartTime { get; set; }

        public double Amount { get; set; } = 3;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? ProviderPaymentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddMinutes(15);
    }
}