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
        [EnumValueDisplayName("Application User")]
        ApplicationUser,

        /// <summary>
        /// Can manage other users
        /// </summary>
        [EnumValueDisplayName("Manage Users")]
        ManageUsers,

        /// <summary>
        /// Can view workflows assigned to others
        /// </summary>
        [EnumValueDisplayName("Can View Workflows Assigned to Other Users")]
        CanViewOtherWorkflows,

        /// <summary>
        /// Can view the Reviews
        /// </summary>
        [EnumValueDisplayName("Can View Review")]
        CanViewReviews,

        /// <summary>
        /// Can create Reviews
        /// </summary>
        [EnumValueDisplayName("Can Create Review")]
        CanCreateReviews,

        [EnumValueDisplayName("Can Execute Reports")]
        CanExecuteReports,
    }
}
