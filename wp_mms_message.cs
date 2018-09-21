using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using NUnit.Framework;

namespace wp2droidMsg
{
    // windows phone and MMS formats are sufficiently different that they need to be read in separately
    public class WpMessageAttachment
    {
        private string m_sContentType;
        private string m_sData;

        public WpMessageAttachment() { }

        #region Comparators
        public override bool Equals(Object obj)
        {
            return obj is WpMessageAttachment && this == (WpMessageAttachment)obj;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(WpMessageAttachment left, WpMessageAttachment right)
        {
            return !(left == right);
        }

        public static bool operator ==(WpMessageAttachment left, WpMessageAttachment right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                if (ReferenceEquals(left, right))
                    return true;
                return false;
            }

            if (left.m_sData != right.m_sData)
                return false;

            if (left.m_sContentType != right.m_sContentType)
                return false;

            return true;
        }
        #endregion

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

        static WpMessageAttachment AttCreateFromString(string s)
        {
            string[] rgs = s.Split('|');
            WpMessageAttachment att = new WpMessageAttachment();

            att.m_sContentType = XmlIO.Nullable(rgs[0]);
            att.m_sData = XmlIO.Nullable(rgs[1]);

            return att;
        }

        [TestCase(null, "<null>|<null>", XmlNodeType.Element, null, null)]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment>", "test/plain|test", XmlNodeType.None, null, null)]
        [Test]
        public static void CreateFromXmlReader(string sIn, string sAttExpected, XmlNodeType ntExpectedNext, string sElementExpectedNext, string sExpectedException)
        {
            WpMessageAttachment att = AttCreateFromString(sAttExpected);

            if (sIn == null)
            {
                Assert.AreEqual(att, new WpMessageAttachment());
                return;
            }

            XmlReader xr = XmlIO.SetupXmlReaderForTest(sIn);

            try
            {
                XmlIO.AdvanceReaderToTestContent(xr, "MessageAttachment");
            }
            catch (Exception e)
            {
                if (sExpectedException != null)
                    return;

                throw e;
            }

            if (sExpectedException == null)
                Assert.AreEqual(att, CreateFromXmlReader(xr));
            if (sExpectedException != null)
                XmlIO.RunTestExpectingException(() => CreateFromXmlReader(xr), sExpectedException);

            Assert.AreEqual(ntExpectedNext, xr.NodeType);
            if (sElementExpectedNext != null)
                Assert.AreEqual(sElementExpectedNext, xr.Name);
        }
    }

    public class WpMessage
    {
        
    }
}