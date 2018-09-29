using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace wp2droidMsg
{
    // windows phone and MMS formats are sufficiently different that they need to be read in separately
    public class WpMessageAttachment
    {
        private string m_sContentType;
        private string m_sData;

        public string ContentType => m_sContentType;
        public string Data => m_sData;

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

        #region Construction
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

            if (xr.NodeType != XmlNodeType.Element || xr.Name != "MessageAttachment")
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
        #endregion

        #region TESTS

        public static WpMessageAttachment AttCreateFromString(string s, char chSep = '|')
        {
            string[] rgs = s.Split(chSep);
            WpMessageAttachment att = new WpMessageAttachment();

            att.m_sContentType = XmlIO.FromNullable(rgs[0]);
            att.m_sData = XmlIO.FromNullable(rgs[1]);

            return att;
        }
        [TestCase(null, "<null>|<null>", XmlNodeType.Element, null, null)]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<Message><MessageAttachment><!-- comment to skip --><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain<!-- comment to skip --></AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String><!-- comment to skip --></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><!-- comment to skip --></Message>", "test/plain|test", XmlNodeType.Comment, "", null)]    // we only consume the attachment, nothing following it
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><!-- comment to skip --><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "test/plain|test", XmlNodeType.EndElement, "Message", null)]
        [TestCase("<MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment>", "test/plain|test", XmlNodeType.None, null, null)]
        [Test]
        public static void TestCreateFromXmlReader(string sIn, string sAttExpected, XmlNodeType ntExpectedNext, string sElementExpectedNext, string sExpectedException)
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
        #endregion
    }

    public class WpMessage
    {
        private string[] m_rgsRecipients;
        private string m_sBody;
        private bool m_fIncoming;
        private bool m_fRead;
        private ulong m_ulLocalTimestamp;
        private string m_sSender;

        private List<WpMessageAttachment> m_platt;

        public string[] Recipients => m_rgsRecipients;
        public string Body => m_sBody;
        public bool Incoming => m_fIncoming;
        public bool Read => m_fRead;
        public ulong LocalTimestamp => m_ulLocalTimestamp;
        public string Sender => m_sSender;

        public List<WpMessageAttachment> Attachments => m_platt;

        public WpMessage() { }

        #region Comparators
        public override bool Equals(Object obj)
        {
            return obj is WpMessage && this == (WpMessage)obj;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(WpMessage left, WpMessage right)
        {
            return !(left == right);
        }

        private static bool FEqArray<T>(T[] rgtLeft, T[] rgtRight) // where T : class
        {
            if (rgtLeft == null && rgtRight == null)
                return true;

            if (rgtLeft == null || rgtRight == null)
                return false;

            if (rgtLeft.Length != rgtRight.Length)
                return false;

            for (int i = 0; i < rgtLeft.Length; i++)
                //if (rgtLeft[i] != rgtRight[i])
                if (!EqualityComparer<T>.Default.Equals(rgtLeft[i], rgtRight[i]))
                    return false;

            return true;
        }

        static bool EqStringArray(string[] rgsLeft, string[] rgsRight)
        {
            if (rgsLeft == null && rgsRight == null)
                return true;

            if (rgsLeft == null || rgsRight == null)
                return false;

            if (rgsLeft.Length != rgsRight.Length)
                return false;

            for (int i = 0; i < rgsLeft.Length; i++)
                if (rgsLeft[i] != rgsRight[i])
                    return false;

            return true;
        }

        public static bool operator ==(WpMessage left, WpMessage right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                if (ReferenceEquals(left, right))
                    return true;
                return false;
            }

            if (left.m_fIncoming != right.m_fIncoming)
                return false;

            if (left.m_fRead != right.m_fRead)
                return false;

            if (left.m_sSender != right.m_sSender)
                return false;

            if (left.m_ulLocalTimestamp != right.m_ulLocalTimestamp)
                return false;
            
            if (!FEqArray<string>(left.m_rgsRecipients, right.m_rgsRecipients))
                return false;

            if (!FEqArray<WpMessageAttachment>(left.m_platt?.ToArray(), right.m_platt?.ToArray())) 
                return false;

            if (left.m_sBody != right.m_sBody)
                return false;

            return true;
        }
        #endregion

        #region Construction

        static void ParseMessageElement(XmlReader xr, WpMessage wpm)
        {
            switch (xr.Name)
            {
                case "Recepients":
                    wpm.m_rgsRecipients = XmlIO.RecepientsReadElement(xr);
                    break;
                case "Body":
                    wpm.m_sBody = XmlIO.ReadGenericStringElement(xr, "Body");
                    break;
                case "IsIncoming":
                    wpm.m_fIncoming = XmlIO.ReadGenericBoolElement(xr, "IsIncoming") ?? false;
                    break;
                case "IsRead":
                    wpm.m_fRead = XmlIO.ReadGenericBoolElement(xr, "IsRead") ?? false;
                    break;
                case "Sender":
                    wpm.m_sSender = XmlIO.ReadGenericStringElement(xr, "Sender");
                    break;
                case "LocalTimestamp":
                    wpm.m_ulLocalTimestamp = XmlIO.ReadGenericUInt64Element(xr, "LocalTimestamp") ?? 0;
                    break;
                case "Attachments":
                    xr.ReadStartElement();  // consume the Attachment element
                    XmlIO.SkipNonContent(xr);

                    if (xr.NodeType != XmlNodeType.Element)
                        throw new Exception("illegal element under Attachments");

                    if (xr.Name != "MessageAttachment")
                        throw new Exception("unexpected element under Attachments");

                    wpm.m_platt = new List<WpMessageAttachment>();
                    while (true)
                    {
                        // if the parser isn't at the right position to consume, then we will throw in CreateFromXmlReader...
                        wpm.m_platt.Add(WpMessageAttachment.CreateFromXmlReader(xr));
                        XmlIO.SkipNonContent(xr);

                        if (xr.NodeType == XmlNodeType.EndElement)
                        {
                            if (xr.Name != "Attachments")
                                throw new Exception("matching end attachment element not found");
                            xr.ReadEndElement();    // consume the </Attachments>
                            //XmlIO.SkipNonContent(xr);
                            break;
                        }
                    }
                    break;
                default:
                    throw new Exception("Unknown element in MessageAttachment");
            }
        }

        public static WpMessage CreateFromXmlReader(XmlReader xr)
        {
            WpMessage att = new WpMessage();

            if (xr.Name != "Message")
                throw new Exception("not at the correct node");

            // finish this start element
            xr.ReadStartElement();

            while (true)
            {
                XmlNodeType nt = xr.NodeType;

                switch (nt)
                {
                    case XmlNodeType.EndElement:
                        if (xr.Name != "Message")
                            throw new Exception("encountered end node not matching <MessageAttachment>");
                        xr.ReadEndElement();
                        return att;

                    case XmlNodeType.Element:
                        ParseMessageElement(xr, att);
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
        #endregion

        #region TESTS
        /*----------------------------------------------------------------------------
        	%%Function: WpMessageFromString
        	%%Qualified: wp2droidMsg.WpMessage.WpMessageFromString
        	%%Contact: rlittle
        	
            format for unit test static construction string is:

            Recip1:Recip2:Recip3|body|incoming|read|att1Type*att1Data:att2Type*att2Data

            note how subgroups have different separators.
        ----------------------------------------------------------------------------*/
        public static WpMessage WpMessageFromString(string s)
        {
            if (s == null)
                return new WpMessage();

            string[] rgs = s.Split('|');
            WpMessage wpm = new WpMessage();

            if (rgs[0] == "<null>")
                wpm.m_rgsRecipients = null;
            else
                wpm.m_rgsRecipients = rgs[0].Split(':');

            wpm.m_sBody = XmlIO.FromNullable(rgs[1]);
            wpm.m_fIncoming = XmlIO.ConvertElementStringToBool(XmlIO.FromNullable(rgs[2])) ?? false;
            wpm.m_fRead = XmlIO.ConvertElementStringToBool(XmlIO.FromNullable(rgs[3])) ?? false;

            if (rgs[4] == "<null>")
                wpm.m_platt = null;
            else
            {
                wpm.m_platt = new List<WpMessageAttachment>();

                string[] rgsAttStrings = rgs[4].Split(':');
                foreach (string sAtt in rgsAttStrings)
                {
                    wpm.m_platt.Add(WpMessageAttachment.AttCreateFromString(sAtt, '*'));
                }
            }

            return wpm;
        }

        [TestCase(null, null, true)]
        [TestCase("<null>|<null>|<null>|<null>|<null>", null, true)]
        [TestCase("<null>|body|<null>|<null>|<null>", "<null>|<null>|<null>|<null>|<null>", false)]
        [TestCase("1234|<null>|<null>|<null>|<null>", "<null>|<null>|<null>|<null>|<null>", false)]
        [TestCase("1234|<null>|<null>|<null>|<null>", "1234|<null>|<null>|<null>|<null>", true)]
        [TestCase("1234:4321|<null>|<null>|<null>|<null>", "1234|<null>|<null>|<null>|<null>", false)]
        [TestCase("1234:4321|<null>|<null>|<null>|<null>", "1234:4321|<null>|<null>|<null>|<null>", true)]
        [TestCase("1234:4321|<null>|<null>|<null>|<null>", "1234:4322|<null>|<null>|<null>|<null>", false)]
        [TestCase("<null>|<null>|<null>|<null>|type/text*data", "<null>|<null>|<null>|<null>|<null>", false)]
        [TestCase("<null>|<null>|<null>|<null>|type/text*data", "<null>|<null>|<null>|<null>|type/text*data", true)]
        [TestCase("<null>|<null>|<null>|<null>|type/text*data:type/foo*data2", "<null>|<null>|<null>|<null>|type/text*data:type/foo*data2", true)]
        [TestCase("<null>|<null>|<null>|<null>|type/text*data:type/foo*data2", "<null>|<null>|<null>|<null>|type/text*data:type/foo*data", false)]
        [Test]
        public static void TestWpMessageComparator(string sLeft, string sRight, bool fExpected)
        {
            WpMessage wpmLeft = WpMessageFromString(sLeft);
            WpMessage wpmRight = WpMessageFromString(sRight);

            Assert.AreEqual(fExpected, wpmLeft == wpmRight);
            Assert.AreEqual(fExpected, wpmLeft.Equals(wpmRight));
            Assert.AreEqual(fExpected, wpmRight.Equals(wpmLeft));
            Assert.AreEqual(!fExpected, wpmLeft != wpmRight);
        }

        [TestCase(null, "<null>|<null>|<null>|<null>|<null>", XmlNodeType.Element, null, null)]
        [TestCase("<Message><Recepients><string>1234</string></Recepients></Message>", "1234|<null>|<null>|<null>|<null>", XmlNodeType.None, null, null)]
        [TestCase("<Message><IsIncoming>true</IsIncoming></Message>", "<null>|<null>|true|<null>|<null>", XmlNodeType.None, null, null)]
        [TestCase("<Message><IsRead>true</IsRead></Message>", "<null>|<null>|<null>|true|<null>", XmlNodeType.None, null, null)]
        [TestCase("<Message><Body>bodytext</Body></Message>", "<null>|bodytext|<null>|<null>|<null>", XmlNodeType.None, null, null)]
        [TestCase("<Message><Body2>bodytext</Body2></Message>", "<null>|bodytext|<null>|<null>|<null>", XmlNodeType.None, null, "System.Exception")]
        [TestCase("<Message><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Message>", "<null>|<null>|<null>|<null>|test/plain*test", XmlNodeType.None, null, "System.Exception")]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><!-- comment to skip --><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><!-- comment to skip --><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><!-- comment to skip --><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType><!-- comment to skip -->test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain<!-- comment to skip --></AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><!-- comment to skip --><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String><!-- comment to skip -->test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test<!-- comment to skip --></AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String><!-- comment to skip --></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><!-- comment to skip --><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><!-- comment to skip --><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType><!-- comment to skip -->test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain<!-- comment to skip --></AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><!-- comment to skip --><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String><!-- comment to skip -->test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2<!-- comment to skip --></AttachmentDataBase64String></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String><!-- comment to skip --></MessageAttachment></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment><!-- comment to skip --></Attachments></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments><!-- comment to skip --></Message>", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.None, null, null)]
        [TestCase("<Message><Attachments><MessageAttachment><AttachmentContentType>test/plain</AttachmentContentType><AttachmentDataBase64String>test</AttachmentDataBase64String></MessageAttachment><MessageAttachment><AttachmentContentType>test2/plain</AttachmentContentType><AttachmentDataBase64String>test2</AttachmentDataBase64String></MessageAttachment></Attachments></Message><!-- comment to skip -->", "<null>|<null>|<null>|<null>|test/plain*test:test2/plain*test2", XmlNodeType.Comment, null, null)]
        [Test]
        public static void TestCreateFromXmlReader(string sIn, string sExpected, XmlNodeType ntExpectedNext, string sEltExpectedNext, string sExpectedException)
        {
            WpMessage wpm = WpMessageFromString(sExpected);

            if (sIn == null)
            {
                Assert.AreEqual(wpm, new WpMessage());
                return;
            }

            XmlReader xr = XmlIO.SetupXmlReaderForTest(sIn);

            try
            {
                XmlIO.AdvanceReaderToTestContent(xr, "Message");
            }
            catch (Exception e)
            {
                if (sExpectedException != null)
                    return;

                throw e;
            }

            if (sExpectedException != null)
            {
                XmlIO.RunTestExpectingException(() => CreateFromXmlReader(xr), sExpectedException);
                return;
            }

            if (sExpectedException == null)
                Assert.AreEqual(wpm, CreateFromXmlReader(xr));

            Assert.AreEqual(ntExpectedNext, xr.NodeType);
            if (sEltExpectedNext != null)
                Assert.AreEqual(sEltExpectedNext, xr.Name);
        }
        #endregion
    }
}