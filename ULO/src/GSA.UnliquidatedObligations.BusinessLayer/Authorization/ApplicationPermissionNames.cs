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
        ApplicationUser,

        /// <summary>
        /// Can manage other users
        /// </summary>
        ManageUsers,

        /// <summary>
        /// Can view workflows assigned to others
        /// </summary>
        CanViewOtherWorkflows,

        /// <summary>
        /// Can view the Reviews
        /// </summary>
        CanViewReviews,
    }
}
