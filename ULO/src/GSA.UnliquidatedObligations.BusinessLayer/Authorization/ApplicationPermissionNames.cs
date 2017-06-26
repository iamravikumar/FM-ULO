using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    /// <summary>
    /// Permissions to use application features
    /// </summary>
    public enum ApplicationPermissionNames
    {
        /// <summary>
        /// Can access the application
        /// </summary>
        [Display(Name = "Application User")]
        ApplicationUser,

        /// <summary>
        /// Can manage other users
        /// </summary>
        [Display(Name = "Manage Users")]
        ManageUsers,

        /// <summary>
        /// Can view workflows assigned to others
        /// </summary>
        [Display(Name = "Can View Workflows Assigned to Other Users")]
        CanViewOtherWorkflows,

        /// <summary>
        /// Can view the Reviews
        /// </summary>
        [Display(Name = "Can View Review")]
        CanViewReviews,

        /// <summary>
        /// Can create Reviews
        /// </summary>
        [Display(Name = "Can Create Review")]
        CanCreateReviews,

        [Display(Name = "Can Execute Reports")]
        CanExecuteReports,
    }
}
