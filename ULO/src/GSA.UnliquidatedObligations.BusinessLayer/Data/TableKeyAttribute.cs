using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class TableKeyAttribute : Attribute
    {
        public TableKeyAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}
