using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;

namespace wp2droidMsg
{
    // windows phone and MMS formats are sufficiently different that they need to be read in separately
    public class WpMessageAttachment
    {
        private string m_sContentType;
        private string m_sData;

        public WpMessageAttachment() { }

        static void ParseMessageAttachmentElement(XmlReader xr, WpMessageAttachment att)
        {
            switch (xr.Name)
            {
                case "AttachmentContentType":
                    att.m_sContentType = XmlIO.ReadGenericStringElement(xr, "AttachmentContentType");
                    break;
                case "AttachmentDataBase64String":
                    att.m_sData = XmlIO.ReadGenericStringElement(xr, "AttachmentDataBase64String");
                    break;
                default:
                    throw new Exception("Unknown element in MessageAttachment");
            }
        }

        public static WpMessageAttachment CreateFromXmlReader(XmlReader xr)
        {
            WpMessageAttachment att = new WpMessageAttachment();

            if (xr.Name != "MessageAttachment")
                throw new Exception("not at the correct node");

            // finish this start element
            xr.ReadStartElement();

            while (true)
            {
                XmlNodeType nt = xr.NodeType;

                switch (nt)
                {
                    case XmlNodeType.EndElement:
                        if (xr.Name != "MessageAttachment")
                            throw new Exception("encountered end node not matching <MessageAttachment>");
                        xr.ReadEndElement();
                        return att;

                    case XmlNodeType.Element:
                        ParseMessageAttachmentElement(xr, att);
                        // we should be advanced past the element...
                        continue;
                    case XmlNodeType.Attribute:
                        throw new Exception("there should be no attributes in this schema");
                }
                // all others just get skipped (whitespace, cdata, etc...)
                if (!xr.Read())
                    break;
            }

            throw new Exception("hit EOF before finding end MessageAttachment element");
        }
    }

    public class WpMessage
    {
        
    }
}