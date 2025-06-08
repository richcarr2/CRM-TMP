using System.Xml.Serialization;

namespace VA.TMP.Integration.Schema.VideoVisitService
{
    public partial class Appointment
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns
        {
            get
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("p", "https://staff.mobilehealth.va.gov/vamf/video-visits/1.0");

                return ns;
            }
            set { /* needed for xml serialization */ }
        }
    }
}