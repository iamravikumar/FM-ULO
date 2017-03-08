namespace GSA.OpenItems.Web
{
    using System;
    using System.Xml;
    using Data;

    public partial class HTTPFunds : PageBase

    {
        private readonly ReportBO Reports;
        private readonly FundStatusBO FundStatus;
        public HTTPFunds()
        {
            Reports = new ReportBO(this.Dal, new EmailsBO(this.Dal));
            FundStatus = new FundStatusBO(this.Dal);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var str_result = "";
            var _page = new PageBase();
            var user = _page.CurrentUserID;

            var obj_xml = new XmlDataDocument();

            try
            {
                obj_xml.Load(Request.InputStream);

                if (obj_xml.DocumentElement.Name == "search_request")
                {
                    Reports.InsertFSRequestResultsByEmail(_page.FundsSearchSelectedValues, user);
                }

                if (obj_xml.DocumentElement.Name == "update_data")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var entry_type = node.Attributes.GetNamedItem("type").Value.Trim();
                    var entry_id = node.Attributes.GetNamedItem("id").Value.Trim();
                    var organization = node.Attributes.GetNamedItem("org").Value.Trim();
                    var fiscal_year = node.Attributes.GetNamedItem("fy").Value.Trim();
                    var month = node.Attributes.GetNamedItem("bm").Value.Trim();
                    var group_code = node.Attributes.GetNamedItem("gcd").Value.Trim();
                    var amount = node.Attributes.GetNamedItem("amount").Value.Trim();
                    var doc_number = node.Attributes.GetNamedItem("doc").Value.Trim();
                    var explanation = node.Attributes.GetNamedItem("exp").Value.Trim();

                    var intEntryType = Int32.Parse(entry_type);
                    var intEntryID = Int32.Parse(entry_id);
                    var mAmount = Decimal.Parse(amount);
                    var intGroupCode = Int32.Parse(group_code);

                    //update data:
                    FundStatus.UpdateUserEntryData(intEntryType, fiscal_year, month, organization, intGroupCode, intEntryID, doc_number, mAmount, explanation, user);

                    //insert history log:
                    var action_code = 0;
                    switch (intEntryType)
                    {
                        case (int)FundsStatusUserEntryType.ueOverUnderAccrued:
                            action_code = (int)HistoryActions.haUpdateOverUnderAccrued;
                            break;
                        case (int)FundsStatusUserEntryType.ueOneTimeAdjustment:
                            action_code = (int)HistoryActions.haUpdateOneTimeAdjustment;
                            break;
                        case (int)FundsStatusUserEntryType.ueExpectedByYearEnd:
                            action_code = (int)HistoryActions.haUpdateExpectedByYearEnd;
                            break;
                    }
                    History.InsertHistoryOnUpdateFundStatusData(action_code, organization, fiscal_year, month, intGroupCode, doc_number, Utility.DisplayMoneyFormat(mAmount), explanation, user);

                    //clear fund status report state:
                    FSSummaryReport.DataObsoleteFlag(fiscal_year, organization);
                }

                if (obj_xml.DocumentElement.Name == "delete_entry")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var entry_type = node.Attributes.GetNamedItem("type").Value.Trim();
                    var entry_id = node.Attributes.GetNamedItem("id").Value.Trim();
                    var organization = node.Attributes.GetNamedItem("org").Value.Trim();
                    var fiscal_year = node.Attributes.GetNamedItem("fy").Value.Trim();
                    var month = node.Attributes.GetNamedItem("bm").Value.Trim();
                    var group_code = node.Attributes.GetNamedItem("gcd").Value.Trim();
                    var amount = node.Attributes.GetNamedItem("amount").Value.Trim();
                    var doc_number = node.Attributes.GetNamedItem("doc").Value.Trim();

                    var intEntryType = Int32.Parse(entry_type);
                    var intEntryID = Int32.Parse(entry_id);
                    var mAmount = Decimal.Parse(amount);

                    //update data:
                    FundStatus.DeleteUserEntryRecord(intEntryID, user);

                    //insert history log:
                    //insert history action as Update not as Delete because of displaying info message to user when last data update has been done.
                    //if in future will be special request on separate history entries - will be changed.
                    var action_code = 0;
                    switch (intEntryType)
                    {
                        case (int)FundsStatusUserEntryType.ueOverUnderAccrued:
                            action_code = (int)HistoryActions.haUpdateOverUnderAccrued;
                            break;
                        case (int)FundsStatusUserEntryType.ueOneTimeAdjustment:
                            action_code = (int)HistoryActions.haUpdateOneTimeAdjustment;
                            break;
                        case (int)FundsStatusUserEntryType.ueExpectedByYearEnd:
                            action_code = (int)HistoryActions.haUpdateExpectedByYearEnd;
                            break;
                    }
                    History.InsertHistoryOnUpdateFundStatusData(action_code, organization, fiscal_year, month, Int32.Parse(group_code), doc_number, Utility.DisplayMoneyFormat(mAmount), "the record has been deleted", user);

                    //clear fund status report state:
                    FSSummaryReport.DataObsoleteFlag(fiscal_year, organization);
                }

                if (obj_xml.DocumentElement.Name == "recalc_report")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var business_line = node.Attributes.GetNamedItem("bl").Value;
                    var organization = node.Attributes.GetNamedItem("org").Value;
                    var fiscal_year = node.Attributes.GetNamedItem("fy").Value;

                    //rebuild fund status report state:
                    FundStatus.RecalculateFSReport(fiscal_year, organization, business_line);
                    History.InsertHistoryOnRebuildFSReport(fiscal_year, organization, business_line, user);
                }

                if (obj_xml.DocumentElement.Name == "recalc_rwa")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var organization = node.Attributes.GetNamedItem("org").Value;
                    var fiscal_year = node.Attributes.GetNamedItem("fy").Value;
                    var month = node.Attributes.GetNamedItem("m").Value;
                    var projection = node.Attributes.GetNamedItem("rwa").Value;

                    if (FundStatus.UpdateRWAProjection(fiscal_year, month, organization, projection, user))
                    {
                        FSSummaryReport.DataObsoleteFlag(fiscal_year, organization);
                        History.InsertHistoryOnUpdateRWAProjection(organization, fiscal_year, month, projection, user);
                    }
                }


                if (str_result == "")
                    str_result = "<ok/>";
            }
            catch (Exception ex)
            {
                str_result = "<error><err_msg msg='" + ex.Message + "' /></error>";
            }

            Response.ContentType = "text/xml";
            Response.Write(str_result);
            Response.Flush();

        }
    }
}