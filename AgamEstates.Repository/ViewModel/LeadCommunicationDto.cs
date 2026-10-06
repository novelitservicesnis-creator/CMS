using System;

namespace AgamEstates.Repository.ViewModel
{
    public class LeadCommunicationDto
    {
        public int CommunicationId { get; set; }
        public int LeadId { get; set; }
        public string? LeadName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Message { get; set; }
        public string? CommunicationType { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLeadCommunicationDto
    {
        public int LeadId { get; set; }
        public int UserId { get; set; }
        public string? Message { get; set; }
        public string? CommunicationType { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }

    public class UpdateLeadCommunicationDto
    {
        public int CommunicationId { get; set; }
        public string? Message { get; set; }
        public string? CommunicationType { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }
}
