using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GSA.UnliquidatedObligations.BusinessLayer.Helpers
{
    public static class XmlHelpers
    {
        public static string WriteObject(this DataContractSerializer ser, object o)
        {
            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(new StringWriter(sb)))
            {
                ser.WriteObject(w, o);
                w.Flush();
            }
            return sb.ToString();
        }

        public static object ReadObject(this DataContractSerializer ser, string xml)
        {
            using (var r = XmlReader.Create(new StringReader(xml)))
            {
                return ser.ReadObject(r);
            }
        }
    }
}
