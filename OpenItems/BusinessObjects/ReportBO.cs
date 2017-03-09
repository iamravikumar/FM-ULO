using System.Collections.Generic;
using System.Linq;
using Data;
using OpenItems.Data;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Collections;
    using System.Configuration;

    /// <summary>
    /// Summary description for ReportBO
    /// </summary>
    /// 


    public enum OIReports
    {
        rpDaily = 1,
        rpDARA = 2,
        rpCOTotal = 3,
        rpUniversityTotal = 4,
        rpDocuments = 5,
        rpValidationByLine = 6,
        rpRegSearchExcel = 7,
        rpValHistSearchExcel = 8,
        rpDaraByDocNum = 9
    }

    public class ReportBO
    {
        private IReportDataLayer Dal;
        private EmailsBO Email;

        public ReportBO(IReportDataLayer dal, EmailsBO email)
        {
            Dal = dal;
            Email = email;
        }


        public List<spGetHistoryByEmailRequest_Result> GetHistoryRecordByEmailRequest(int iEmailRequestID)
        {
            return Dal.GetHistoryByEmailRequest(iEmailRequestID).ToList();
        }

        public void InsertFSRequestResultsByEmail(Hashtable htCriteriaValues, int CurrentUserID)
        {

            var criteria_fields = "";
            var criteria_values = "";

            foreach (DictionaryEntry de in htCriteriaValues)
            {
                criteria_fields += de.Key + "|";
                criteria_values += de.Value + "|";
            }

            var email_request = Email.InsertEmailRequest(CurrentUserID, (int)HistoryActions.haRequestFSSearchResultsByEmail, true);
            History.InsertHistoryOnFSRequestSearchResultsByEmail(criteria_fields, criteria_values, email_request, CurrentUserID);
        }

        public DataSet GetDocumentsReport(int iLoadID)
        {
            return Dal.GetReportDocuments(iLoadID);
        }

        public DataSet GetCOTotalReport(int iLoadID)
        {
            return Dal.GetReportCOTotal(iLoadID);
        }


        public DataSet GetValidationByLineReport(int iLoadID)
        {
            return Dal.GetReportValidationByLine(iLoadID);
        }

        public List<spReportDaily_Result> GetDailyReport(int iLoadID)
        {
            return Dal.GetReportDaily(iLoadID).ToList();
        }

        public List<spReportTotalSum_Result> GetTotalSumReport(int iLoadID)
        {
            return Dal.GetReportTotalSum(iLoadID).ToList();
        }

        public List<spReportTotalByValid_Result> GetTotalSumReportByValid(int iLoadID)
        {
            return Dal.GetReportTotalByValid(iLoadID).ToList();
        }

        public List<spReportTotalByOrg_Result> GetTotalByOrganization(int iLoadID)
        {
            return Dal.GetTotalByOrganization(iLoadID).ToList();
        }

        public List<spDaraByDocNum_Result> GetTotalDaraNew(int iLoadID)
        {
            return Dal.GetDaraByDocNum(iLoadID).ToList();

        }
    }
}