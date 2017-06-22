using System;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class TypeHelpers
    {
        /// <summary>
        /// Determines whether or not the object is a whole number
        /// </summary>
        /// <param name="t">The type we are testing</param>
        /// <returns>True if it is a whole number, else false</returns>
        public static bool IsWholeNumber(this Type t)
        {
            return (
                       t == typeof(Int16) ||
                       t == typeof(Int32) ||
                       t == typeof(Int64) ||
                       t == typeof(UInt16) ||
                       t == typeof(UInt32) ||
                       t == typeof(UInt64) ||
                       t == typeof(SByte) ||
                       t == typeof(Byte));
        }

        /// <summary>
        /// Determines whether or not the object is a real number
        /// </summary>
        /// <param name="t">The type we are testing</param>
        /// <returns>True if it is a real number, else false</returns>
        public static bool IsRealNumber(this Type t)
        {
            return (
                       t == typeof(Single) ||
                       t == typeof(Double) ||
                       t == typeof(Decimal));
        }

        /// <summary>
        /// Determines whether or not the object is a number
        /// </summary>
        /// <param name="t">The type we are testing</param>
        /// <returns>True if it is a number, else false</returns>
        public static bool IsNumber(this Type t)
        {
            return IsWholeNumber(t) || IsRealNumber(t);
        }
    }
}
