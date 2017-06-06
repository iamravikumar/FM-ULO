using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.UploadTable
{
    public static class Empty
    {
        public static readonly Attribute[] AttributeArray = new Attribute[0];
        public static readonly byte[] ByteArray = new byte[0];
        public static readonly Guid[] GuidArray = new Guid[0];
        public static readonly int[] IntArray = new int[0];
        public static readonly Int64[] Int64Array = new Int64[0];
        public static readonly object[] ObjectArray = new object[0];
        public static readonly Regex[] RegexArray = new Regex[0];
        public static readonly string[] StringArray = new string[0];
        public static readonly Type[] TypeArray = new Type[0];
        public static readonly uint[] UIntArray = new uint[0];
        public static readonly Uri[] UriArray = new Uri[0];
        public static readonly Version Version = new Version(0, 0, 0, 0);
        public static readonly IDictionary Dictionary = new EmptyHashtable();
        public static readonly NameValueCollection NameValueCollection = new EmptyNameValueCollection();
        public static readonly SqlInt64 SqlInt64 = new SqlInt64();
        public static readonly SqlInt32 SqlInt32 = new SqlInt32();

        #region Empty Classes

        #region Nested type: EmptyHashtable

        private class EmptyHashtable : Hashtable
        {
            public override bool IsReadOnly
            {
                get { return true; }
            }
        }

        #endregion

        #region Nested type: EmptyNameValueCollection

        private class EmptyNameValueCollection : NameValueCollection
        {
            public EmptyNameValueCollection()
            {
                base.IsReadOnly = true;
            }
        }

        #endregion

        #endregion
    }
}
