using System;
using System.Collections.Generic;
using System.Data;

namespace GSA.UnliquidatedObligations.Utility
{
    public class LoadRowsFromExcelSettings : LoadRowsSettings
    {
        public bool UseSheetNameForTableName { get; set; }
        public int? SheetNumber { get; set; }
        public string SheetName { get; set; }
        public int SkipRawRows { get; set; }
        public bool TreatAllValuesAsText { get; set; }

        public LoadRowsFromExcelSettings() { }

        public LoadRowsFromExcelSettings(LoadRowsSettings other)
            : base(other)
        { }

        public LoadRowsFromExcelSettings(LoadRowsFromExcelSettings other)
            : base(other)
        {
            if (other == null) return;
            this.UseSheetNameForTableName = other.UseSheetNameForTableName;
            this.SheetNumber = other.SheetNumber;
            this.SheetName = other.SheetName;
            this.SkipRawRows = other.SkipRawRows;
            this.TreatAllValuesAsText = other.TreatAllValuesAsText;
        }
    }

    public class LoadSheetsFromExcelSettings
    {
        public List<LoadRowsFromExcelSettings> SheetSettings;
        public LoadRowsSettings LoadAllSheetsDefaultSettings;
        public Func<DataTable> CreateDataTable;
    }
}
