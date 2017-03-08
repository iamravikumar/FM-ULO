namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Collections;
    using System.Configuration;
    using System.Xml;
    using System.Web.UI.WebControls;
    using Data;

    public class FundAllowanceBO
    {

        private readonly IFundAllowanceDataLayer Dal;

        public FundAllowanceBO(IFundAllowanceDataLayer dal)
        {
            Dal = dal;
        }
        public DataSet GetDetailedAllowanceForFiscalYear(string sFiscalYear)
        {
            return Dal.GetFYAllowance(sFiscalYear);
        }

        public ArrayList GetAvailableFiscalYearList(string sAppPath)
        {
            var al = new ArrayList();
            var doc = new XmlDocument();
            doc.Load(sAppPath + "//FundStatus//include//FS_SupportedFiscalYears.xml");
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                al.Add(new ListItem(node.InnerText));
            }
            return al;
        }

        //revised
        public DataTable GetYearTotalAllowance(string sFiscalYear)
        {
            return Dal.GetFYAllowanceTotals(sFiscalYear).Tables[0];
        }

        //revised
        public DataTable GetYearAllowanceForOrganization(string sFiscalYear, string sOrg)
        {
            return Dal.GetFYAllowanceTotalsOrg(sFiscalYear, sOrg).Tables[0];
        }

        //revised
        public DataTable GetYearAllowanceForBusinessLine(string sFiscalYear, string sBusinessLine)
        {
            return Dal.GetFYAllowanceTotalsBL(sFiscalYear, sBusinessLine).Tables[0];
        }

        //revised
        public static DataTable GetYearDistribution(string sFiscalYear)
        {
            return FSDataServices.GetYearDistribution(sFiscalYear).Tables[0];
        }
        //revised
        public static DataTable GetYearDistributionForBusinessLine(string sFiscalYear, string sBusinessLine)
        {
            return FSDataServices.GetYearDistributionForBusinessLine(sFiscalYear, sBusinessLine).Tables[0];
        }
        //revised
        public static DataTable GetYearDistributionForOrganization(string sFiscalYear, string sOrgCode)
        {
            return FSDataServices.GetYearDistributionForOrganization(sFiscalYear, sOrgCode).Tables[0];
        }

        public int SaveFYAllowance(int iAllowRecordID, string sFiscalYear, decimal dAmount, string sMonthList,int iMonthCount, int iUpdateUserID)
        {
            return Dal.SaveFYAllowance(iAllowRecordID, sFiscalYear, dAmount, sMonthList, iMonthCount, iUpdateUserID);
        }

        public DataSet GetAllowanceHistory(string sFiscalYear, HistoryActions code1, HistoryActions code2)
        {
            return Dal.GetAllowanceHistory(sFiscalYear, code1, code2);
        }

        public DataSet GetFundStatusUpdateHistory(HistoryActions code, string sFisalYear, string sBookMonth,string sOrg)
        {
            return Dal.GetFundStatusUpdateHistory2(code, sFisalYear, sBookMonth, sOrg);
        }

        public decimal GetMonthlyAmount(string FiscalYear, string BookMonth, string BusinessLine, string OrgCode, int DistributionType)
        {

            DataTable dt;
            decimal total_monthly = 0;

            if (DistributionType == (int)FundAllowanceViewType.vtAllowanceTotal)
                dt = GetYearTotalAllowance(FiscalYear);
            else if (DistributionType == (int)FundAllowanceViewType.vtAllowanceForBusinessLine)
                dt = GetYearAllowanceForBusinessLine(FiscalYear, BusinessLine);
            else
                dt = GetYearAllowanceForOrganization(FiscalYear, OrgCode);

            var dr_col = dt.Select(String.Format("BookMonth='{0}'", BookMonth));
            if (dr_col.Length > 0)
                total_monthly = (decimal)dr_col[0]["Amount"];

            return total_monthly;

        }
    }
}