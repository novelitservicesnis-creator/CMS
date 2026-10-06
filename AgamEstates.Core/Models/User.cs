namespace AgamEstates.Core.Models
{
    using System;
    using System.Collections.Generic;

    public partial class User
    {
        public User()
        {
            AssignedLeads = new HashSet<Lead>();
            Communications = new HashSet<LeadCommunication>();
        }

        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public string Role { get; set; } = "Agent";
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Lead> AssignedLeads { get; set; }
        public virtual ICollection<LeadCommunication> Communications { get; set; }
    }
}
