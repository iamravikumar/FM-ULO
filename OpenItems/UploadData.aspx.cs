namespace GSA.OpenItems.Web
{
    using System;
    using System.Web.UI.WebControls;
    using System.IO;

    public partial class UploadData : PageBase 
    {
        private readonly Lookups LookupsBO;
        private readonly UploadServiceBO UploadService;
        public UploadData()
        {
            LookupsBO = new Lookups(this.Dal, new AdminBO(this.Dal));
            UploadService = new UploadServiceBO(this.Dal);
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            lblMsg.Text = "";

            if (!IsPostBack)
            {
                tr_date_of_report.AddDisplayNone();

                var dataSourceTypes = LookupsBO.GetDataSourceTypes();
                ddlDataSource.DataSource = dataSourceTypes;
                ddlDataSource.DataValueField = "DataSource";
                ddlDataSource.DataTextField = "DataSourceDescription";
                ddlDataSource.DataBind();

                var openItemTypes = LookupsBO.GetOpenItemTypes();
                ddlItemType.DataSource = openItemTypes;
                ddlItemType.DataValueField = "OIType";
                ddlItemType.DataTextField = "OITypeDescription";
                ddlItemType.DataBind();
                ddlItemType.Items.Insert(0, "- select -");
                ddlItemType.SelectedValue = "- select -";  //default
                ddlItemType_SelectedIndexChanged(null, null);

                //dtDue.MinDate = DateTime.Now;
                dtDue.MaxDate = DateTime.Now.AddYears(2);
                //dtDue.VoidEmpty = true;
                //dtDue.FreezeMode = false;
                //dtDue.VoidEdit = false;
                //dtDue.Date = DateTime.Now;

                string[] round_arr = { "2", "3", "4", "5", "6", "7", "8", "9" };
                ddlRound.DataSource = round_arr;
                ddlRound.DataBind();
            }
        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            this.btnUpload.Click += new EventHandler(btnUpload_Click);
            this.ddlItemType.SelectedIndexChanged += new EventHandler(ddlItemType_SelectedIndexChanged);
        }

        void ddlItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var dt = LookupsBO.GetLoadList().Tables[0];

                //update ddlDates:
                var col = new ListItemCollection();
                ListItem it;
                if (ddlItemType.SelectedIndex != 0)
                {
                    btnUpload.Visible = true;
                    var strOIType = ddlItemType.SelectedValue;
                    var select = String.Format("OIType = {0} and ParentLoadID = 0", strOIType);
                    var drows = dt.Select(select, "LoadDate DESC");
                    string _value;
                    string _text;
                    it = new ListItem("", "0");
                    col.Add(it);
                    foreach (var dr in drows)
                    {
                        _value = dr["LoadID"].ToString();
                        _text = "Load Date: " + ((DateTime)dr["LoadDate"]).ToString("MMM dd, yyyy") + ", " + dr["LoadDesc"].ToString() + ", DueDate: " + ((DateTime)dr["DueDate"]).ToString("MMM dd, yyyy");

                        if (dr["ArchiveDate"] != DBNull.Value)
                            _text = _text + " (Archived)";
                        it = new ListItem(_text, _value);
                        col.Add(it);
                    }
                    if (ddlItemType.SelectedValue == "6")
                    {
                        tr_date_of_report.AddDisplay();
                    }
                    else
                    {
                        tr_date_of_report.AddDisplayNone();
                    }

                }
                else
                {
                    btnUpload.Visible = false;
                }
                if (col.Count == 1)
                {
                    it = new ListItem("There is no loaded data found", "0");
                    col.Add(it);
                }
                ddlPrevLoad.DataSource = col;
                ddlPrevLoad.DataValueField = "Value";
                ddlPrevLoad.DataTextField = "Text";
                ddlPrevLoad.DataBind();                
                
            }
            catch (Exception ex)
            {
                //lblError.Text = ex.Message;
            }            
        }

        private string ValidateCalendarControl(Controls.GetDate oCalendarControl, string sFieldName, string sError)
        {
            if (oCalendarControl.isEmpty == true)
            {
                if (sError == "")
                {
                    sError = "Please enter '" + sFieldName + "'";
                }
                else
                {
                    sError = sError + "<br>Please enter '" + sFieldName + "'";
                }
                oCalendarControl.DateCSSClass = "dateYellow";
            }
            else
            {
                oCalendarControl.DateCSSClass = "date";
            }

            return sError;
        }

        private string ValidateTextBox(TextBox oTextBox, string sFieldName, string sError)
        {
            if (oTextBox.Text.Trim() == "")
            {
                if (sError == "")
                {
                    sError = "Please enter '" + sFieldName + "'";
                }
                else
                {
                    sError = sError + "<br>Please enter '" + sFieldName + "'";
                }
                oTextBox.AddStyle("background-color:Yellow");
            }
            else
            {
                oTextBox.AddStyle("background-color:white");
            }
            return sError;
        }

        void btnUpload_Click(object sender, EventArgs e)
        {
            string sPath;
            var sError = "";

            sError = ValidateCalendarControl(dtDue, "Due Date", sError);
            sError = ValidateTextBox(txtReviewName, "Review Name", sError);

            var report_date = DateTime.Now;

            //if (ddlItemType.SelectedIndex == 6)
            //{
            //    sError = ValidateCalendarControl(dtReportDate, "Date of Report", sError);
            //    report_date = dtReportDate.Date;
            //}

            try
            {
                var due_date = dtDue.Date;

                

                ApplicationAssert.CheckCondition(ddlDataSource.SelectedIndex > -1, "Please select the data source.");
                ApplicationAssert.CheckCondition(ctrlFileUpload.HasFile, "Please select Excel file to upload.");
                var sFileName = Server.HtmlEncode(ctrlFileUpload.FileName);
                var sExtension = System.IO.Path.GetExtension(sFileName);
                ApplicationAssert.CheckCondition(sExtension == ".xls", "Only Excel files may be uploaded.");

                sPath = Server.MapPath("~\\UploadedFiles\\" + sFileName);

                //if previous Excel file with the same name exists in the directory UploadFiles - delete it
                var fInfo = new FileInfo(sPath);
                if (fInfo.Exists)
                    fInfo.Delete();
                
                int file_length;
                file_length = (int)ctrlFileUpload.PostedFile.InputStream.Length;

                var file_data = new byte[file_length];

                var content_type = ctrlFileUpload.PostedFile.ContentType;

                ctrlFileUpload.PostedFile.InputStream.Read(file_data, 0, file_length);

                //int intFileID = DataFile.AddNewFile(sFileName, file_length, file_data, content_type);

                var intFileID = FilesBO.AddNewFile(sFileName, file_length, file_data, content_type, CurrentUserLogin);

                ctrlFileUpload.SaveAs(sPath);
                
                var intDataSource = Int32.Parse(ddlDataSource.SelectedValue);
                var intOIType = Int32.Parse(ddlItemType.SelectedValue);
                
                var intParentLoadID = Int32.Parse(ddlPrevLoad.SelectedValue);
                var intRound = Int32.Parse(ddlRound.SelectedValue);

                if (sError == "")
                {
                    UploadService.LoadData(sPath, intFileID, sFileName, intDataSource, intOIType, due_date, intParentLoadID, intRound, CurrentUserID, txtReviewName.Text, report_date);
                    //don't keep these files on the hard disk:
                    fInfo = new FileInfo(sPath);
                    if (fInfo.Exists)
                        fInfo.Delete();
                }
                else
                {
                    lblMsg.CssClass = "error";
                    lblMsg.Text = sError;
                }

            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblMsg.CssClass = "error";
                    //lblError.Text = "SQL Transaction has failed. " + GetErrors() + sError;
                }
                else
                {
                    if(sError == "")
                    {
                    //enabled_textbox
                    lblMsg.CssClass = "green_text_14";
                    lblMsg.Text = "The data was successfully uploaded into the system. Thank you.";
                    }
                }
            }
        }

       
}
}