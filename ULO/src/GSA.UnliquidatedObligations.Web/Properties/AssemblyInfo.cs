﻿using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("GSA.UnliquidatedObligations.Web")]
[assembly: AssemblyDescription(@"The website for the GSA ULO project. 
You can find more information regarding that project at https://github.com/GSA/FM-ULO. 
This uses ASP.NET MVC and right now, is the sole entry point for users of ULO. 
As there are no standalone background applications, Hangfire Server run in-process to 
execute long running background tasks such as the creation of a new review.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("GSA")]
[assembly: AssemblyProduct("GSA.UnliquidatedObligations.Web")]
[assembly: AssemblyCopyright("Copyright © GSA 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7bbb0a59-632c-452e-9e3e-4f40de095eb5")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
