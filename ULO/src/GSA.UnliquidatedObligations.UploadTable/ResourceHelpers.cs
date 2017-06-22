using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class ResourceHelpers
    {
        /// <summary>
        /// Get an embedded resource as a stream
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <returns>The stream, else null</returns>
        public static Stream GetEmbeddedResourceAsStream(string name)
        {
            return GetEmbeddedResourceAsStream(name, Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Get an embedded resource as a stream
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <param name="a">The assembly that houses the resource, if null, uses the caller</param>
        /// <returns>The stream, else null</returns>
        public static Stream GetEmbeddedResourceAsStream(string name, Assembly a)
        {
            if (null == name) return null;
            if (a == null) a = Assembly.GetCallingAssembly();
            string[] streamNames = a.GetManifestResourceNames();
            name = name.ToLower();
            if (Array.IndexOf(streamNames, name) == -1)
            {
                foreach (string streamName in streamNames)
                {
                    if (streamName.EndsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        int i = name.Length + 1;
                        if (streamName.Length < i || streamName[streamName.Length - i] == '.')
                        {
                            name = streamName;
                            break;
                        }
                    }
                }
            }
            return a.GetManifestResourceStream(name);
        }

        /// <summary>
        /// Get an embedded resource as a string
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <returns>The string, else null</returns>
        public static string GetEmbeddedResourceAsString(string name)
        {
            return GetEmbeddedResourceAsString(name, Assembly.GetCallingAssembly());
        }


        /// <summary>
        /// Get an embedded resource as a string
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <param name="a">The assembly that houses the resource, if null, uses the caller</param>
        /// <returns>The string, else null</returns>
        public static string GetEmbeddedResourceAsString(string name, Assembly a)
        {
            using (Stream st = GetEmbeddedResourceAsStream(name, a))
            {
                if (st == null) return null;
                string s = StringFromStream(st, null);
                return s;
            }
        }

        private static string StringFromStream(Stream st, Encoding enc)
        {
            if (null == enc)
            {
                enc = Encoding.UTF8;
            }
            var sr = new StreamReader(st, Encoding.UTF8);
            string s = sr.ReadToEnd();
            return s;
        }

    }
}
