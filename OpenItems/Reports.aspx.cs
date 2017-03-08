namespace GSA.OpenItems.Web
{
    using System;

    public partial class Reports : PageBase
    {
        private GSA.OpenItems.Web.Controls_CriteriaFields oCF;


        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {

                //oCF = (GSA.OpenItems.Web.Controls_CriteriaFields)Page.LoadControl("~/Controls/CriteriaFields.ascx");
                //int iITC = oCF.ItemTypeCode;
                //InitControls();


                if (!IsPostBack)
                {
                    InitControls();

                    btnTotalDaily.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpDaily).ToString() + ");");
                    btnDARA.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpDARA).ToString() + ");");
                    btnDARAByDocNum.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpDaraByDocNum).ToString() + ");");
                    btnCOReport.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpCOTotal).ToString() + ");");
                    btnTotalUniverse.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpUniversityTotal).ToString() + ");");
                    btnDocuments.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpDocuments).ToString() + ");");
                    btnValidationByLine.Attributes.Add("onclick", "javascript:return get_customized_report(" + ((int)OIReports.rpValidationByLine).ToString() + ");");
                }

                ctrlCriteria.DisplayDefaultsRep();

                if (ctrlCriteria.ItemTypeCode == 6 || Session["ReportType"] == "6")
                {
                    btnCOReport.Visible = false;
                    btnDARA.Visible = false;
                    btnDARAByDocNum.Visible = false;
                    btnDocuments.Visible = false;
                    btnTotalDaily.Visible = true;
                    btnCOReport.Visible = true;
                    btnTotalUniverse.Visible = false;
                    btnValidationByLine.Visible = false;
                    btnTotalDaily.Value = "BA53 Total Daily";
                    btnCOReport.Value = "BA53 CO Report";
                    btnTotalUniverse.Visible = false;
                    btnValidationByLine.Visible = false;
                }
                else
                {
                    btnCOReport.Visible = true;
                    btnDARA.Visible = true;
                    btnDARAByDocNum.Visible = true;
                    btnDocuments.Visible = true;
                    btnTotalDaily.Visible = true;
                    btnCOReport.Visible = true;
                    btnTotalUniverse.Visible = true;
                    btnValidationByLine.Visible = true;
                    btnTotalDaily.Value = "Total Daily";
                    btnCOReport.Value = "CO Report";
                }

                if (ctrlCriteria.LoadRound != "1st") // feedback load
                {
                    btnTotalDaily.Visible = false;
                    lblError.Text = "The Total Daily Report is not available for the follow-up reviews";
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        private void InitControls()
        {
            ctrlCriteria.AddWorkloadFilterValue = false;
            ctrlCriteria.DisplaySearchFields = false;
            //ctrlCriteria.ItemTypeIsRequired = true;
            ctrlCriteria.LoadSelectionIsRequired = true;
            ctrlCriteria.DisplaySubmitSection = false;
            ctrlCriteria.InitControls();
            ctrlCriteria.DisplayDefaultsRep();
        }



    }
}