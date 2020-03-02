using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class DynamicTemplateFileProvider : IFileProvider, IDirectoryContents
    {
        private readonly UloDbContext DB;
        private readonly PortalHelpers PortalHelpers;
        private readonly ILogger Log;

        public DynamicTemplateFileProvider(UloDbContext db, PortalHelpers ph, ILogger log)
        {
            DB = db;
            PortalHelpers = ph;
            Log = log;
        }

        private IDictionary<string, ETFolder> GetEmailTemplateFolderByPath()
            => Cache.DataCacher.FindOrCreateValue(
                nameof(DynamicTemplateFileProvider),
                () => DB.EmailTemplates.AsNoTracking().ConvertAll(z=>new ETFolder(z)).ToDictionary(z => z.PhysicalPath),
                PortalHelpers.MediumCacheTimeout);

        IDirectoryContents IFileProvider.GetDirectoryContents(string subpath)
        {
            if (subpath=="/Pages" || subpath=="/Views") return this;
            return GetEmailTemplateFolderByPath().GetValue(subpath);
        }

        IFileInfo IFileProvider.GetFileInfo(string subpath)
        {
            IFileInfo ret = null;
            foreach (var dir in GetEmailTemplateFolderByPath().Values.OfType<IDirectoryContents>())
            {
                foreach (var file in dir)
                {
                    if (file.PhysicalPath == subpath)
                    {
                        ret = file;
                        goto Found;
                    }
                }
            }
            Found:
            Log.Information("Looking for {subPath} => {file}", subpath, ret);
            return ret;
        }

        private class NullChangeToken : BaseDisposable, IChangeToken
        {
            public static readonly IChangeToken Instance = new NullChangeToken();

            private NullChangeToken() { }

            bool IChangeToken.HasChanged => false;

            bool IChangeToken.ActiveChangeCallbacks => false;

            IDisposable IChangeToken.RegisterChangeCallback(Action<object> callback, object state)
               => this;
        }

        IChangeToken IFileProvider.Watch(string filter)
        {
            if (filter.Contains(RootEmailTemplatePath))
            {
                Stuff.Noop(filter);
            }
            return NullChangeToken.Instance;
        }

        bool IDirectoryContents.Exists => true;

        IEnumerator<IFileInfo> IEnumerable<IFileInfo>.GetEnumerator()
            => GetEmailTemplateFolderByPath().Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEmailTemplateFolderByPath().Values.GetEnumerator();

        private const string RootEmailTemplatePath = "/Views/Email";

        private class ETFolder : IDirectoryContents, IFileInfo
        {
            private readonly EmailTemplate EmailTemplate;
            public readonly int EmailTemplateId;
            private readonly IList<IFileInfo> Files;

            public bool Exists => true;

            long IFileInfo.Length => 0;

            public string Name { get; }

            public string PhysicalPath => $"{RootEmailTemplatePath}/{Name}";

            DateTimeOffset IFileInfo.LastModified => Stuff.ApplicationStartedAt;

            bool IFileInfo.IsDirectory => true;

            public ETFolder(EmailTemplate emailTemplate)
            {
                EmailTemplate = emailTemplate;
                EmailTemplateId = emailTemplate.EmailTemplateId;
                Name = $"{EmailTemplateId}";
                Files = new[] {
                    new TemplateItem(this, nameof(EmailTemplate.EmailBody), EmailTemplate.EmailBody),
                    new TemplateItem(this, nameof(EmailTemplate.EmailHtmlBody), EmailTemplate.EmailHtmlBody),
                    new TemplateItem(this, nameof(EmailTemplate.EmailSubject), EmailTemplate.EmailSubject)
                };
            }

            IEnumerator<IFileInfo> IEnumerable<IFileInfo>.GetEnumerator()
                => Files.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => Files.GetEnumerator();

            Stream IFileInfo.CreateReadStream()
                => throw new NotSupportedException();

            private class TemplateItem : IFileInfo
            {
                private readonly string Template;

                public TemplateItem(ETFolder folder, string name, string template)
                {
                    Name = $"{name}.cshtml";
                    PhysicalPath = $"{folder.PhysicalPath}/{Name}";
                    Template = template;
                    using (var st = CreateReadStream())
                    {
                        Length = st.Length;
                    }
                }

                bool IFileInfo.Exists => true;

                public long Length { get; }

                public string PhysicalPath { get; }

                public string Name { get; }


                private static readonly IDictionary<string, DateTimeOffset> LastModifiedCache = new Dictionary<string, DateTimeOffset>();

                DateTimeOffset IFileInfo.LastModified
                    => LastModifiedCache.FindOrCreate(
                        Cache.CreateKey(PhysicalPath, Length, Template),
                        () => DateTimeOffset.UtcNow);

                bool IFileInfo.IsDirectory => false;

                public Stream CreateReadStream()
                    => StreamHelpers.Create(Template);
            }
        }
    }
}
