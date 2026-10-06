using System;

namespace AgamEstates.Repository.ViewModel
{
    public class LeadDto
    {
        public int LeadId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PropertyInterest { get; set; }
        public decimal? Budget { get; set; }
        public string? City { get; set; }
        public string? Source { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string Priority { get; set; } = "Medium";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLeadDto
    {
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
    }

    public class UpdateLeadDto
    {
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
    }
}
