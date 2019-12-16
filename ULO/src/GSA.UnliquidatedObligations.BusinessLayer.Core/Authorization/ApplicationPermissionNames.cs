using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    /// <summary>
    /// Permissions to use application features
    /// </summary>
    public enum ApplicationPermissionNames
    {
        [Display(Name = "Application User", Description = "Required in order to use this ULO application.")]
        ApplicationUser,

        [Display(Name = "Manage Users", Description = "Create and edit users and privileges.")]
        ManageUsers,

        [Display(Name = "View Users", Description = "View users and priviliges.")]
        ViewUsers,

        [Display(Name = "View Reviews", Description = "Access to the \"Reviews\" screen.")]
        CanViewReviews,

        [Display(Name = "Search", Description = "Access to the \"Search\" screen.")]
        CanViewOtherWorkflows,

        [Display(Name = "Manage Reviews", Description ="Launch new reviews.")]
        CanCreateReviews,

        [Display(Name = "Execute Reports", Description ="Run reports from the \"Reports\" screen.")]
        CanExecuteReports,

        [Display(Name = "Schedule Reports", Description = "Schedule reports for recurring email distribution from the \"Reports\" screen.")]
        CanScheduleReports,

        [Display(Name = "Manage Jobs", Description = "System administrative functions.")]
        BackgroundJobDashboard,

        [Display(Name = "Reassign ULOs", Description = "View the \"Reassignments\" screen and choose a specific person to own the workload.")]
        CanReassign,

        [Display(Name = "View Unassigned", Description = "Access to the \"Unassigned\" screen.")]
        CanViewUnassigned,
    }
}
