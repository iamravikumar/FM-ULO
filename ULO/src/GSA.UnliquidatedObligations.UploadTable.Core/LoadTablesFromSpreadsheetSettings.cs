using System;
using System.Collections.Generic;
using System.Data;

namespace GSA.UnliquidatedObligations.Utility
{
    public class LoadTablesFromSpreadsheetSettings
    {
        public List<LoadRowsFromSpreadsheetSettings> SheetSettings;
        public LoadRowsSettings LoadAllSheetsDefaultSettings;
        public Func<DataTable> CreateDataTable;
    }
}
