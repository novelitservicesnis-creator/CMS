namespace AgamEstates.Core.Models
{
    using System;

    public partial class LeadCommunication
    {
        public int CommunicationId { get; set; }
        public int LeadId { get; set; }
        public int UserId { get; set; }
        public string? Message { get; set; }
        public string? CommunicationType { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Lead? Lead { get; set; }
        public virtual User? User { get; set; }
    }
}
