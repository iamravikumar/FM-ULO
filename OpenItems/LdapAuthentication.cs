using System;
using System.Text;
using System.DirectoryServices;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace GSA.BESS.Web.Classes
{
    public class LdapAuthentication
	{
		private string sPath;
		private string sFilterAttribute;

		public LdapAuthentication(string path)
		{
			sPath = path;  
		}

		public bool IsAuthenticated(string sDomain, string sUsername, string sPwd, Page p, bool bLogin, out string sUserEmail)
		{
      
			var bPassedLDAP = false;
            var bNullValues = false;
			var sDomainAndsUsername = sDomain + @"\" + sUsername.Trim();   //using network login name
			//string sDomainAndsUsername = sUsername; // using email address
			var entry = new DirectoryEntry(sPath, sDomainAndsUsername, sPwd.Trim());

			var sEmail = "";
            //string sParticipantId = "0";
            //string sPersonnelId = "0";
            //string sOrgID = "0";
            //string sOrgName = "";
            //string sPhone = "";
            //string sRole = "4";
            //string sActive = "1";
            //string sSupervisorEmail = "";
            //string sFName = "";
            //string sMI = "";
            //string sLName = "";
            //string sSupervisorName = "";

			try
			{
                if (sUsername.Trim() == "" || sPwd.Trim() == "")
                {
                    bNullValues = true;
                    bPassedLDAP = false;
                    throw new Exception("UserName and Password fields should not be null");
                }
                else
                {

                    var obj = entry.NativeObject; //Bind to the native AdsObject to force authentication.

                    var oSearch = new DirectorySearcher(entry);

                    oSearch.Filter = "(SAMAccountName=" + sUsername + ")";   //using network login name
                    //oSearch.Filter = "(userPrincipalName=" + sUsername + ")";  // using email address
                    oSearch.PropertiesToLoad.Add("cn"); // network login name
                    oSearch.PropertiesToLoad.Add("userPrincipalName");  //
                    oSearch.PropertiesToLoad.Add("givenName");  //first name
                    oSearch.PropertiesToLoad.Add("mail");  //
                    oSearch.PropertiesToLoad.Add("initials");  //middle initial
                    oSearch.PropertiesToLoad.Add("sn");  //last name

                    var oResult = oSearch.FindOne();

                    if (null == oResult)
                    {
                        bPassedLDAP = false;
                    }
                    //Update the new path to the user in the directory.
                    sPath = oResult.Path;

                    sFilterAttribute = (string)oResult.Properties["cn"][0];



                    try
                    {
                        sEmail = (string)oResult.Properties["mail"][0];
                    }
                    catch (Exception ex)
                    {
                        bPassedLDAP = false;
                        throw new Exception("Network account error: Your network account is not configured properly. The LDAP 'mail' object doesn't returned a valid GSA email address. Please contact the IT Service Desk at 866-450-5250, and provide them with this message. (This is not the ULO application error)");
                    }

                    // !!!!!!! FOR TEST ONLY !!!!!!!!//
                    //sEmail = "moges.ayalew@gsa.gov";  // for testing only 
                    ////////////////////////////////////


                    if (sEmail != null && sEmail != "")
                    {
                        sEmail = sEmail.Trim().ToLower();
                        bPassedLDAP = true;
                    }
                    else
                    {
                        bPassedLDAP = false;
                        throw new Exception("Network account error: Your network account is not configured properly. Your user name and password are correct, but the LDAP 'mail' object returned null value instead of a valid GSA email address. Please contact the IT Service Desk at 866-450-5250, and provide them with this message. (This is not the ULO application error)");

                    }

                    // !!!!!!! FOR TEST ONLY !!!!!!!!//
                    //sEmail = "@gsa.gov";  // for testing only 
                    ////////////////////////////////////

                    if (IsValidEmailAddress(sEmail) == true)
                    {
                        bPassedLDAP = true;
                    }
                    else
                    {
                        //bPassedLDAP = false;
                        bPassedLDAP = false;
                        throw new Exception("Network account error: Your GSA network account is not setup properly. Your user name and password are correct, but the LDAP 'mail' object returned an invalid GSA email address: '" + sEmail + "'. Please contact the IT Service Desk at 866-450-5250, and provide them with this message. (This is not the ULO application error)");
                    }
                }


			}
			catch (Exception ex)
			{
                if (bPassedLDAP == false && bNullValues==false) // LDAP error
				{
					throw new Exception("LDAP authenticating error. " + ex.Message);

				}	
			    else if (bPassedLDAP == false && bNullValues==true)
                {
                    throw new Exception("UserName and Password fields should not be null");
                }
			}

			sUserEmail = sEmail;
			return true;
		}
		public static bool IsValidEmailAddress(string sEmail)
		{
			if (sEmail == null)
			{
				return false;
			}

			var nFirstAT = sEmail.IndexOf('@');
			var nLastAT = sEmail.LastIndexOf('@');

			if ((nFirstAT > 0) && (nLastAT == nFirstAT) &&
				(nFirstAT < (sEmail.Length - 1)))
			{
				// address is ok regarding the single @ sign
				//return (Regex.IsMatch(sEmail, @"(\w+)@(\w+)\.(\w+)"));
				return (Regex.IsMatch(sEmail, @"(\w+)"+"@gsa.gov"));
			}
			else
			{
				return false;
			}
		}
     



		public string GetGroups()
		{
			var oSearch = new DirectorySearcher(sPath);
			oSearch.Filter = "(cn=" + sFilterAttribute + ")";
			oSearch.PropertiesToLoad.Add("memberOf");
			var groupNames = new StringBuilder();

			try
			{
				var oResult = oSearch.FindOne();
				var propertyCount = oResult.Properties["memberOf"].Count;
				string dn;
				int equalsIndex, commaIndex;

				for (var propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
				{
					dn = (string)oResult.Properties["memberOf"][propertyCounter];
					equalsIndex = dn.IndexOf("=", 1);
					commaIndex = dn.IndexOf(",", 1);
					if (-1 == equalsIndex)
					{
						return null;
					}
					groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
					groupNames.Append("|");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error obtaining group names. " + ex.Message);
			}
			return groupNames.ToString();
		}
	}
}
