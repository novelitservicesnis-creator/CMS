namespace AgamEstates.Core.Models
{
    using System;
    using System.Collections.Generic;

    public partial class LeadStatus
    {
        public LeadStatus()
        {
            Leads = new HashSet<Lead>();
        }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Lead> Leads { get; set; }
    }
}
