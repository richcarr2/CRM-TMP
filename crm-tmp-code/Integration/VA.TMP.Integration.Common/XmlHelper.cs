using System.IO;
using System.Xml;

namespace VA.TMP.Integration.Common
{
    public static class XmlHelper
    {
        public static XmlDocument LoadParameters(string xml)
        {
            XmlDocument doc = null;
            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using (var xr = XmlReader.Create(new StringReader(xml), settings))
            {
                doc = new XmlDocument();
                doc.Load(xr);
            }

            return doc;
        }
    }
}