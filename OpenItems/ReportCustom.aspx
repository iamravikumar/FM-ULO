<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Reports/ReportsGlobal.master" AutoEventWireup="true" CodeFile="ReportCustom.aspx.cs" Inherits="GSA.OpenItems.Web.ReportCustom" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
		<meta content="C#" name="CODE_LANGUAGE" />
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
		
		<!--#include virtual="../include/ReportExcel.xml" -->
		<style type="text/css"><!--#include file="../css/report_excel.css" --></style>
		<style type="text/css">
            .reportHeaderBlue
            {
	            text-align: center;
	            font-size: 12px;
	            font-weight: bold;
	            color: #FFFFFF;
	            background-color: #677388;
	            padding-top: 3px;
	            padding-bottom: 3px;
	            padding-left:1px;
	            padding-right:1px;
            }
            
            .reportHeaderGreen
            {
	            text-align: center;
	            font-size: 12px;
	            font-weight: bold;
	            color: #FFFFFF; 
	            padding-top: 3px;
	            padding-bottom: 3px;
	            padding-left: 1px;
	            padding-right: 1px;
	            background-color: #507950;
	            /*border-left: dimgray 1px solid;*/
            }
            
            .header { mso-number-format:"\@"; FONT-WEIGHT: bold;  font-size:12px; COLOR:Navy ; TEXT-ALIGN: center; vertical-align:bottom }

		</style>
</asp:Content>
