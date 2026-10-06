using AgamEstates.Repository.ViewModel;
using System;
using System.Collections.Generic;

namespace AgamEstates.Web.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class EnquiryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PropertyInterest { get; set; }
        public decimal? Budget { get; set; }
        public string? City { get; set; }
        public string? Message { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int InterestedLeads { get; set; }
        public int SiteVisitsScheduled { get; set; }
        public int NegotiationLeads { get; set; }
        public int ClosedWonLeads { get; set; }
        public int FollowUpsDue { get; set; }

        public Dictionary<string, int> PipelineCounts { get; set; } = new();
        public List<LeadDto> RecentLeads { get; set; } = new();
        public List<LeadCommunicationDto> UpcomingFollowUps { get; set; } = new();
    }

    public class LeadsPageViewModel
    {
        public List<LeadDto> Leads { get; set; } = new();
        public List<LeadStatusDto> Statuses { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();

        public string? Search { get; set; }
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public int? AssignedToFilter { get; set; }
        public string? SourceFilter { get; set; }
    }

    public class LeadDetailsPageViewModel
    {
        public LeadDto Lead { get; set; } = null!;
        public List<LeadCommunicationDto> Communications { get; set; } = new();
        public List<LeadStatusDto> Statuses { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
    }

    public class UsersPageViewModel
    {
        public List<UserDto> Users { get; set; } = new();
        public string? Search { get; set; }
        public string? RoleFilter { get; set; }
        public bool? ActiveFilter { get; set; }
        public Dictionary<int, int> UserLeadCounts { get; set; } = new();
        public int TotalManageableCount { get; set; }
        public int ActiveManageableCount { get; set; }
        public int InactiveManageableCount { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Url { get; set; }

        public BreadcrumbItem() { }

        public BreadcrumbItem(string text, string? url = null)
        {
            Text = text;
            Url = url;
        }
    }
}
