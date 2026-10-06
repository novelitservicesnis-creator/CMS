namespace AgamEstates.Core.Models
{
    using System;
    using System.Collections.Generic;

    public partial class Lead
    {
        public Lead()
        {
            Communications = new HashSet<LeadCommunication>();
        }

        public int LeadId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PropertyInterest { get; set; }
        public decimal? Budget { get; set; }
        public string? City { get; set; }
        public string? Source { get; set; }
        public int? StatusId { get; set; }
        public int? AssignedTo { get; set; }
        public string Priority { get; set; } = "Medium";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual LeadStatus? Status { get; set; }
        public virtual User? AssignedUser { get; set; }
        public virtual ICollection<LeadCommunication> Communications { get; set; }
    }
}
