using System.Xml.Serialization;

namespace VA.TMP.Integration.Schema.VirtualMeetingRoom
{
    public partial class VirtualMeetingRoomType
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns
        {
            get
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("vmr", "http://va.gov/vyopta/schemas/exchange/VirtualMeetingRoom/1.0");

                return ns;
            }
            set { /* needed for xml serialization */ }
        }
    }
}