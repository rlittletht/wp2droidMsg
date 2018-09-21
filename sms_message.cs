
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;


namespace wp2droidMsg
{
    public class SmsMessage
    {
        private int m_protocol;
        private string m_sAddress;
        private ulong m_msecUnixDate;
        private int m_type;
        private string m_sSubject;
        private string m_sBody;
        private string m_sToa;
        private string m_sSc_Toa;
        private string m_sServiceCenter;
        private int m_nRead;
        private int m_nStatus;
        private int m_nLocked;
        private string m_sReadableDate;
        string m_sContactName;

        public SmsMessage() { }

        #region Comparators
        public override bool Equals(Object obj)
        {
            return obj is SmsMessage && this == (SmsMessage) obj;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(SmsMessage left, SmsMessage right)
        {
            return !(left == right);
        }

        public static bool operator ==(SmsMessage left, SmsMessage right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                if (ReferenceEquals(left, right))
                    return true;
                return false;
            }

            if (left.m_protocol != right.m_protocol)
                return false;
            if (left.m_protocol != right.m_protocol)
                return false;
            if (left.m_sAddress != right.m_sAddress)
                return false;
            if (left.m_msecUnixDate != right.m_msecUnixDate)
                return false;
            if (left.m_type != right.m_type)
                return false;
            if (left.m_sSubject != right.m_sSubject)
                return false;
            if (left.m_sBody != right.m_sBody)
                return false;
            if (left.m_sToa != right.m_sToa)
                return false;
            if (left.m_sSc_Toa != right.m_sSc_Toa)
                return false;
            if (left.m_sServiceCenter != right.m_sServiceCenter)
                return false;
            if (left.m_nRead != right.m_nRead)
                return false;
            if (left.m_nStatus != right.m_nStatus)
                return false;
            if (left.m_nLocked != right.m_nLocked)
                return false;
            if (left.m_sReadableDate != right.m_sReadableDate)
                return false;
            if (left.m_sContactName != right.m_sContactName)
                return false;

            return true;
        }
        #endregion

        #region XML I/O

        public void WriteToDroidXml(XmlWriter xw)
        {
            xw.WriteStartElement("sms");
            xw.WriteAttributeString("protocol", m_protocol.ToString());
            xw.WriteAttributeString("address", m_sAddress ?? "");
            xw.WriteAttributeString("date", m_msecUnixDate.ToString());
            xw.WriteAttributeString("type", m_type.ToString());
            xw.WriteAttributeString("subject", m_sSubject ?? "null");
            xw.WriteAttributeString("body", m_sBody ?? "null" );
            xw.WriteAttributeString("toa", m_sToa ?? "null");
            xw.WriteAttributeString("sc_toa", m_sSc_Toa ?? "null");
            xw.WriteAttributeString("service_center", m_sServiceCenter ?? "null");
            xw.WriteAttributeString("read", m_nRead.ToString());
            xw.WriteAttributeString("status", m_nStatus.ToString());
            xw.WriteAttributeString("locked", m_nLocked.ToString());
            xw.WriteEndElement();
        }

        /*----------------------------------------------------------------------------
        	%%Function: MsecUnixFromSecondWin
        	%%Qualified: wp2droidMsg.SmsMessage.MsecUnixFromSecondWin
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static ulong MsecUnixFromSecondWin(ulong nWpDate)
        {
            return (((nWpDate / (10 * 100)) - 116444736000000) + 5) / 10;
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateFromXmlReader
        	%%Qualified: wp2droidMsg.SmsMessage.CreateFromXmlReader
        	%%Contact: rlittle
        	
            given an XmlReader that is current positioned at the start of an
            <Message> element, parse until the </Message> and return a new SmsMessage.
        ----------------------------------------------------------------------------*/
        public static SmsMessage CreateFromWindowsPhoneXmlReader(XmlReader xr)
        {
            SmsMessage sms = new SmsMessage();

            sms.m_type = 2;    // absence of the IsIncoming element meants it is a sent text, hence 2
            sms.m_nStatus = -1; // always -1 as far as I can tell

            if (xr.Name != "Message")
                throw new Exception("not at the correct node");

            // finish this start element
            xr.ReadStartElement();

            while(true)
            {
                XmlNodeType nt = xr.NodeType;

                switch (nt)
                {
                    case XmlNodeType.EndElement:
                        if (xr.Name != "Message")
                            throw new Exception("encountered end node not matching <Message>");
                        xr.ReadEndElement();
                        return sms;

                    case XmlNodeType.Element:
                        ParseMessageElement(xr, sms);
                        // we should be advanced past the element...
                        continue;
                    case XmlNodeType.Attribute:
                        throw new Exception("there should be no attributes in this schema");
                }
                // all others just get skipped (whitespace, cdata, etc...)
                if (!xr.Read())
                    break;
            } 

            throw new Exception("hit EOF before finding end Message element");
        }

        /*----------------------------------------------------------------------------
        	%%Function: ParseMessageElement
        	%%Qualified: wp2droidMsg.SmsMessage.ParseMessageElement
        	%%Contact: rlittle
        	
            the parser should be positioned at an xml start element, ready for us
            to parse the element into the given sms
        ----------------------------------------------------------------------------*/
        static void ParseMessageElement(XmlReader xr, SmsMessage sms)
        {
            switch (xr.Name)
            {
                case "Recepients":
                    string sRecipients = XmlIO.RecepientsReadElement(xr);
                    if (sRecipients != null)
                        sms.m_sAddress = sRecipients;
                    // if null, then don't change m_sAddress...
                    break;
                case "Body":
                    sms.m_sBody = XmlIO.ReadGenericStringElement(xr, "Body");
                    break;
                case "IsIncoming":
                    bool? fIncoming = XmlIO.ReadGenericBoolElement(xr, "IsIncoming");
                    if (fIncoming == null)
                        break; // no change
                    if ((bool) fIncoming)
                        sms.m_type = 1;
                    else
                        sms.m_type = 2;
                    break;
                case "IsRead":
                    bool? fRead = XmlIO.ReadGenericBoolElement(xr, "IsRead");
                    if (fRead == null)
                        break;

                    sms.m_nRead = ((bool) fRead) ? 1 : 0;
                    break;
                case "Attachments":
                    xr.Skip();
                    // TODO TEST THIS!!!
                    break;
                case "LocalTimestamp":
                    ulong? ulRead = XmlIO.ReadGenericUInt64Element(xr, "LocalTimestamp");
                    if (ulRead == null)
                        break;

                    sms.m_msecUnixDate = MsecUnixFromSecondWin((ulong) ulRead);
                    break;
                case "Sender":
                    string sSender = XmlIO.ReadGenericStringElement(xr, "Sender");
                    if (sSender == null)
                        break;
                    sms.m_sAddress = sSender;
                    break;
                default:
                    throw new Exception("Unknown element in Message");
            }
        }
        #endregion

        #region TESTS

        [TestCase(131271698820000000UL, 1482696282000UL)]
        [TestCase(131272856426710000UL, 1482812042671UL)]
        [TestCase(131305673581450420UL, 1486093758145UL)]
        [TestCase(131777420586331428UL, 1533268458633UL)]
        [TestCase(131777420698276081UL, 1533268469828UL)]
        [Test]
        public static void TestMsecUnixFromSecondWin(ulong nWpDate, ulong nExpected)
        {
            Assert.AreEqual(nExpected, MsecUnixFromSecondWin(nWpDate));
        }


        static SmsMessage SmsCreateFromString(string s)
        {
            string[] rgs = s.Split('|');
            SmsMessage sms = new SmsMessage();

            sms.m_protocol = int.Parse(rgs[0]);
            sms.m_sAddress = XmlIO.Nullable(rgs[1]);
            sms.m_msecUnixDate = UInt64.Parse(rgs[2]);
            sms.m_type = int.Parse(rgs[3]);
            sms.m_sSubject = XmlIO.Nullable(rgs[4]);
            sms.m_sBody = XmlIO.Nullable(rgs[5]);
            sms.m_sToa = XmlIO.Nullable(rgs[6]);
            sms.m_sSc_Toa = XmlIO.Nullable(rgs[7]);
            sms.m_sServiceCenter = XmlIO.Nullable(rgs[8]);
            sms.m_nRead = int.Parse(rgs[9]);
            sms.m_nStatus = int.Parse(rgs[10]);
            sms.m_nLocked = int.Parse(rgs[11]);
            sms.m_sReadableDate = XmlIO.Nullable(rgs[12]);
            sms.m_sContactName = XmlIO.Nullable(rgs[13]);

            return sms;
        }

        // Order is:    nProtocol|sAddress|nUnixDate|nType|sSubject|sBody|sToa|sSc_toa|sServiceCenter|nRead|nStatus|nLocked|nDateSent|sReadableDate|sContactName
        [TestCase(null, "0|<null>|0|0|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Recepients><string>+1234</string></Recepients></Message>", "0|+1234|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing</Body></Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><IsIncoming>true</IsIncoming></Message>", "0|<null>|0|1|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><IsRead>1</IsRead></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|1|-1|0|<null>|<null>", null)]
        [TestCase("<Message><LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|<null>|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender></Message>", "0|+4321|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender><LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender> <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender>\r\n <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender><!-- comment here --> <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body><![CDATA[testing]]></Body></Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing\nnewline</Body></Message>", "0|<null>|0|2|<null>|testing\nnewline|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Recepients><string>+1234</string></Recepients><Body>foo&amp;bar</Body></Message>", "0|+1234|0|2|<null>|foo&bar|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing</Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|-1|0|<null>|<null>", "System.Xml.XmlException")]
        [TestCase("<Message><Recepients><string>+14255551212</string></Recepients><Body>:-)</Body><IsIncoming>false</IsIncoming><IsRead>true</IsRead><Attachments /><LocalTimestamp>131777420698276081</LocalTimestamp><Sender /></Message>", "0|+14255551212|1533268469828|2|<null>|:-)|<null>|<null>|<null>|1|-1|0|<null>|<null>", null)]
        [TestCase("<Message></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Unknown>foobar</Unknown></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", "System.Exception")]
        [Test]
        public static void TestCreateFromWindowsPhoneXmlReader(string sIn, string sSmsExpected,
            string sExpectedException)
        {
            SmsMessage smsExpected = SmsCreateFromString(sSmsExpected);

            if (sIn == null)
            {
                Assert.AreEqual(smsExpected, new SmsMessage());
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

            if (sExpectedException == null)
                Assert.AreEqual(smsExpected, CreateFromWindowsPhoneXmlReader(xr));
            if (sExpectedException != null)
                XmlIO.RunTestExpectingException(() => CreateFromWindowsPhoneXmlReader(xr), sExpectedException);
        }

        [Test]
        public static void TestXmlReaderFull()
        {
            string sTest =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Message><Recepients><string>+14254956002</string></Recepients><Body>:-)</Body><IsIncoming>false</IsIncoming><IsRead>true</IsRead><Attachments /><LocalTimestamp>131777420698276081</LocalTimestamp><Sender /></Message></ArrayOfMessage>";

            XmlReader xr = XmlIO.SetupXmlReaderForTest(sTest);
            XmlIO.AdvanceReaderToTestContent(xr, "Message");
        }
        #endregion
    }
}
