using System;
using Microsoft.Extensions.Configuration;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web
{
    public class GsaAdministrativeSettings
    {
        public static class KeyNames
        {
            public const string APPSETTINGS_DIRECTORY = "APPSETTINGS_DIRECTORY";
            public const string LOGFILE_DIRECTORY = "LOGFILE_DIRECTORY";
            public const string TEMPFILE_DIRECTORY = "TEMPFILE_DIRECTORY";
            public const string ATTACHMENT_DIRECTORY = "ATTACHMENT_DIRECTORY";
        }
        public string AppSettingsDirectory { get; }
        public string LogFileDirectory { get; }
        public string TempFileDirectory { get; }
        public string AttachmentDirectory { get; }

        private string Expand(string s)
        {
            s = StringHelpers.TrimOrNull(s);
            if (s != null)
            {
                if (TempFileDirectory != null)
                {
                    s = s.Replace("%temp%", TempFileDirectory, StringComparison.InvariantCultureIgnoreCase);
                }
                s = Environment.ExpandEnvironmentVariables(s);
            }
            return s;
        }

        public GsaAdministrativeSettings(IConfiguration configuration)
        {
            TempFileDirectory = Expand(configuration[KeyNames.TEMPFILE_DIRECTORY]);
            AppSettingsDirectory = Expand(configuration[KeyNames.APPSETTINGS_DIRECTORY]);
            LogFileDirectory = Expand(configuration[KeyNames.LOGFILE_DIRECTORY]);
            AttachmentDirectory = Expand(configuration[KeyNames.ATTACHMENT_DIRECTORY]);
        }
    }

    public static class GsaAdministrativeSettingsHelpers
    {
        public static GsaAdministrativeSettings GetGsaAdministrativeSettings(this IConfiguration configuration)
            => new GsaAdministrativeSettings(configuration);
    }
}
