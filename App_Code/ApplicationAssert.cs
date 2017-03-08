namespace GSA.OpenItems.Web
{
    public static class ApplicationAssert
    {
        /// <summary>
        ///     Verify that a required condition holds. 
        ///     <param name="condition">An expression to be tested for True</param>
        ///     <param name="errorText">The message to display</param>
        ///     <exception class="System.ApplicationException">
        ///         The checked condition failed.    
        ///     </exception>
        /// </summary>
        public static void CheckCondition(bool condition, string errorText)
        {
            //Test the condition
            if (!condition)
            {
                //Assert and throw if the condition is not met
                throw new AppError(errorText);
            }
        }
    }
}