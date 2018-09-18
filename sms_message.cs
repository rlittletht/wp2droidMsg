
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

        #region XML I/O

        static string StringElementReadFromXml(XmlReader xr)
        {
            return xr.ReadElementContentAsString("string", "");
        }

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
                    string sRecipients = RecepientsReadElement(xr);
                    if (sRecipients != null)
                        sms.m_sAddress = sRecipients;
                    // if null, then don't change m_sAddress...
                    break;
                case "Body":
                    sms.m_sBody = ReadGenericStringElement(xr, "Body");
                    break;
                case "IsIncoming":
                    bool? fIncoming = ReadGenericBoolElement(xr, "IsIncoming");
                    if (fIncoming == null)
                        break; // no change
                    if ((bool) fIncoming)
                        sms.m_type = 1;
                    else
                        sms.m_type = 2;
                    break;
                case "IsRead":
                    bool? fRead = ReadGenericBoolElement(xr, "IsRead");
                    if (fRead == null)
                        break;

                    sms.m_nRead = ((bool) fRead) ? 1 : 0;
                    break;
                case "Attachments":
                    xr.Skip();
                    // TODO TEST THIS!!!
                    break;
                case "LocalTimestamp":
                    ulong? ulRead = ReadGenericUInt64Element(xr, "LocalTimestamp");
                    if (ulRead == null)
                        break;

                    sms.m_msecUnixDate = MsecUnixFromSecondWin((ulong) ulRead);
                    break;
                case "Sender":
                    string sSender = ReadGenericStringElement(xr, "Sender");
                    if (sSender == null)
                        break;
                    sms.m_sAddress = sSender;
                    break;
                default:
                    throw new Exception("Unknown element in Message");
            }
        }

        /*----------------------------------------------------------------------------
        	%%Function: RecepientsReadElement
        	%%Qualified: wp2droidMsg.SmsMessage.RecepientsReadElement
        	%%Contact: rlittle
        	
            this is either empty or has a single <string> element as a child
        ----------------------------------------------------------------------------*/
        static string RecepientsReadElement(XmlReader xr)
        {
            if (xr.Name != "Recepients")
                throw new Exception("not at the correct node");

            if (xr.IsEmptyElement)
            {
                xr.ReadStartElement(); // since this is both the start and an empty element
                return null;
            }

            // read for the child
            while (xr.Read())
            {
                XmlNodeType nt = xr.NodeType;

                if (nt == XmlNodeType.EndElement)
                {
                    if (xr.Name != "Recepients")
                        throw new Exception("encountered end node not matching <Recepients>");

                    // this just means that it had child text nodes that didn't matter (like whitespace or comments)
                    // its ok, just advance reader past it and return
                    xr.ReadEndElement();
                    return null;
                }

                if (nt == XmlNodeType.Element)
                {
                    string s = StringElementReadFromXml(xr).Trim();

                    // now we should be at the EndElement for Recepients
                    if (xr.Name != "Recepients")
                        throw new Exception("not at the correct node");

                    xr.ReadEndElement();
                    return s;
                }
            }

            throw new Exception("didn't find string child in recepients");
        }

        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericStringElement
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericStringElement
        	%%Contact: rlittle
        	
            read the givent element as a string
        ----------------------------------------------------------------------------*/
        static string ReadGenericStringElement(XmlReader xr, string sElement)
        {
            if (xr.Name != sElement)
                throw new Exception("not at the correct node");

            if (xr.IsEmptyElement)
            {
                xr.ReadStartElement(); // since this is both the start and an empty element
                return null;
            }

            // read for the child
            string s = xr.ReadElementContentAsString().Trim();
            if (String.IsNullOrEmpty(s))
                return null;

            // ReadElementContentAsString advances past the end element, so the parse should
            // be all set. 
            return s;
        }

        /*----------------------------------------------------------------------------
        	%%Function: ConvertElementStringToInt
        	%%Qualified: wp2droidMsg.SmsMessage.ConvertElementStringToInt
        	%%Contact: rlittle
        	
            convert the given string into an integer
        ----------------------------------------------------------------------------*/
        static int? ConvertElementStringToInt(string sElementString)
        {
            if (sElementString != null)
                return int.Parse(sElementString);

            return null;
        }

        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericIntElement
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericIntElement
        	%%Contact: rlittle
        	
            read the given element as an integer
        ----------------------------------------------------------------------------*/
        static int ?ReadGenericIntElement(XmlReader xr, string sElement)
        {
            return ConvertElementStringToInt(ReadGenericStringElement(xr, sElement));
        }

        /*----------------------------------------------------------------------------
        	%%Function: ConvertElementStringToUInt64
        	%%Qualified: wp2droidMsg.SmsMessage.ConvertElementStringToUInt64
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static UInt64? ConvertElementStringToUInt64(string sElementString)
        {
            if (sElementString != null)
                return UInt64.Parse(sElementString);

            return null;
        }

        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericUInt64Element
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericUInt64Element
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static UInt64? ReadGenericUInt64Element(XmlReader xr, string sElement)
        {
            return ConvertElementStringToUInt64(ReadGenericStringElement(xr, sElement));
        }

        /*----------------------------------------------------------------------------
        	%%Function: ConvertElementStringToBool
        	%%Qualified: wp2droidMsg.SmsMessage.ConvertElementStringToBool
        	%%Contact: rlittle
        	
            convert the given string into a bool
        ----------------------------------------------------------------------------*/
        static bool? ConvertElementStringToBool(string sElementString)
        {
            if (sElementString == null)
                return null;

            if (sElementString == "0" || sElementString == "false")
                return false;
            if (sElementString == "1" || sElementString == "true")
                return true;

            throw new System.FormatException($"{sElementString} is not a boolean value");
        }

        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericBoolElement
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericBoolElement
        	%%Contact: rlittle
        	
            Read the given element as a boolean
        ----------------------------------------------------------------------------*/
        static bool? ReadGenericBoolElement(XmlReader xr, string sElement)
        {
            return ConvertElementStringToBool(ReadGenericStringElement(xr, sElement));
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

        /*----------------------------------------------------------------------------
        	%%Function: SetupXmlReaderForTest
        	%%Qualified: wp2droidMsg.SmsMessage.SetupXmlReaderForTest
        	%%Contact: rlittle
        	
            take a static string representing an XML snippet, and wrap an XML reader
            around the string

            NOTE: This is not very efficient -- it decodes the string into bytes, then
            creates a memory stream (which ought to be disposed of eventually since
            it is based on IDisposable), and then we finally return. But, these are 
            tests and will run fast enough. Don't steal this code for production
            though.
        ----------------------------------------------------------------------------*/
        public static XmlReader SetupXmlReaderForTest(string sTestString)
        {
            return XmlReader.Create(new StringReader(sTestString));
        }

        public static void AdvanceReaderToTestContent(XmlReader xr, string sElementTest)
        {
            XmlNodeType nt;

            while (xr.Read())
            {
                nt = xr.NodeType;
                if (nt == XmlNodeType.Element && xr.Name == sElementTest)
                    return;
            }

            throw new Exception($"could not advance to requested element '{sElementTest}'");
        }

        static void RunTestExpectingException(TestDelegate pfn, string sExpectedException)
        {
            if (sExpectedException == "System.Xml.XmlException")
                Assert.Throws<System.Xml.XmlException>(pfn);
            else if (sExpectedException == "System.Exception")
                Assert.Throws<System.Exception>(pfn);
            else if (sExpectedException == "System.OverflowException")
                Assert.Throws<System.OverflowException>(pfn);
            else if (sExpectedException == "System.ArgumentException")
                Assert.Throws<System.ArgumentException>(pfn);
            else if (sExpectedException == "System.FormatException")
                Assert.Throws<System.FormatException>(pfn);
            else if (sExpectedException != null)
                throw new Exception("unknown exception type");
        }

        [TestCase("<string>test</string>", "test", null)]
        [TestCase("\r\n<string>test</string>", "test", null)]
        [TestCase("<string>\rtest</string>", "\ntest", null)]
        [TestCase("<foo>test</foo>", null, "System.Exception")]
        [TestCase("<string><foo>test</foo></string>", null, "System.Xml.XmlException")]
        [Test]
        public static void TestStringElementReadFromXml(string sTest, string sExpectedReturn, string sExpectedException)
        {
            XmlReader xr = SetupXmlReaderForTest(sTest);
            try
            {
                AdvanceReaderToTestContent(xr, "string");
            }
            catch (Exception e)
            {
                if (sExpectedException != null)
                    return;
                throw e;
            }

            if (sExpectedException == null)
                Assert.AreEqual(sExpectedReturn, StringElementReadFromXml(xr));
            if (sExpectedException != null)
                RunTestExpectingException(() => StringElementReadFromXml(xr), sExpectedException);
        }

        // NOTE: This does NOT test if the parser is left in a good state!!
        [TestCase("<Recepients><string>+14253816865</string></Recepients>", "+14253816865", null)]
        [TestCase("<Recepients><string>\r+14253816865</string></Recepients>", "+14253816865", null)]
        [TestCase("<Recepients><string>(425) 381-6865</string></Recepients>", "(425) 381-6865", null)]
        [TestCase("<Recepients>\r\n<string>+14253816865</string></Recepients>", "+14253816865", null)]
        [TestCase("<Recepients />", null, null)]
        [TestCase("<Recepients>\r\n\t</Recepients>", null, null)]
        [TestCase("<Recepients></Recepients>", null, null)]
        [TestCase("<Recepients><string2>+14253816865</string2></Recepients>", null, "System.Xml.XmlException")]
        [TestCase("<Recepients><foo/><string>+14253816865</string></Recepients>", null, "System.Xml.XmlException")]
        [Test]
        public static void TestRecepientsReadElement(string sTest, string sExpectedReturn, string sExpectedException)
        {
            XmlReader xr = SetupXmlReaderForTest(sTest);
            try
            {
                AdvanceReaderToTestContent(xr, "Recepients");
            }
            catch (Exception e)
            {
                if (sExpectedException != null)
                    return;
                throw e;
            }

            if (sExpectedException == null)
                Assert.AreEqual(sExpectedReturn, RecepientsReadElement(xr));
            if (sExpectedException != null)
                RunTestExpectingException(() => RecepientsReadElement(xr), sExpectedException);
        }

        [TestCase("<bar><Recepients><string>1234</string></Recepients><foo/></bar>", XmlNodeType.Element, "foo")]
        [TestCase("<Recepients><string>1234</string></Recepients> ", XmlNodeType.Whitespace, null)]
        [TestCase("<Recepients/> ", XmlNodeType.Whitespace, null)]
        [TestCase("<bar><Recepients/><foo/></bar>", XmlNodeType.Element, "foo")]
        [TestCase("<bar><Recepients> </Recepients><foo/></bar>", XmlNodeType.Element, "foo")]
        [TestCase("<bar><Recepients/> <foo/></bar>", XmlNodeType.Whitespace, null)]
        [Test]
        public static void TestRecepientsReadElementParserReturnState(string sTest, XmlNodeType ntExpected,
            string sNameExpected)
        {
            XmlReader xr = SetupXmlReaderForTest(sTest);
            AdvanceReaderToTestContent(xr, "Recepients");

            RecepientsReadElement(xr);
            Assert.AreEqual(ntExpected, xr.NodeType);
            if (sNameExpected != null)
                Assert.AreEqual(sNameExpected, xr.Name);
        }

        // NOTE: This does NOT test if the parser is left in a good state!!
        [TestCase("<Foo>+14253816865</Foo>", "Foo", "+14253816865", null)]
        [TestCase("<Recepients>\r+14253816865</Recepients>", "Recepients", "+14253816865", null)]
        [TestCase("<Recepients>(425) 381-6865</Recepients>", "Recepients", "(425) 381-6865", null)]
        [TestCase("<Recepients>\r\n+14253816865</Recepients>", "Recepients", "+14253816865", null)]
        [TestCase("<Recepients />", "Recepients", null, null)]
        [TestCase("<Recepients>\r\n\t</Recepients>", "Recepients", null, null)]
        [TestCase("<Recepients></Recepients>", "Recepients", null, null)]
        [TestCase("<Recepients>+14253816865</Recepients>", "Foo", null, "System.Xml.XmlException")]
        [TestCase("<Recepients><foo/>+14253816865</Recepients>", "Recepients", null, "System.Xml.XmlException")]
        [Test]
        public static void TestReadGenericStringElement(string sTest, string sExpectedElement, string sExpectedReturn, string sExpectedException)
        {
            XmlReader xr = SetupXmlReaderForTest(sTest);
            try
            {
                AdvanceReaderToTestContent(xr, sExpectedElement);
            }
            catch (Exception e)
            {
                if (sExpectedException != null)
                    return;
                throw e;
            }

            if (sExpectedException == null)
                Assert.AreEqual(sExpectedReturn, ReadGenericStringElement(xr, sExpectedElement));
            if (sExpectedException != null)
                RunTestExpectingException(() => ReadGenericStringElement(xr, sExpectedElement), sExpectedException);
        }

        [TestCase("<bar><Recepients>1234</Recepients><foo/></bar>", "Recepients", XmlNodeType.Element, "foo")]
        [TestCase("<Recepients>1234</Recepients> ", "Recepients", XmlNodeType.Whitespace, null)]
        [TestCase("<Recepients/> ", "Recepients", XmlNodeType.Whitespace, null)]
        [TestCase("<bar><Recepients/><foo/></bar>", "Recepients", XmlNodeType.Element, "foo")]
        [TestCase("<bar><Recepients> </Recepients><foo/></bar>", "Recepients", XmlNodeType.Element, "foo")]
        [TestCase("<bar><Recepients/> <foo/></bar>", "Recepients", XmlNodeType.Whitespace, null)]
        [Test]
        public static void TestReadGenericStringParserReturnState(string sTest, string sExpectedElement, XmlNodeType ntExpected,
            string sNameExpected)
        {
            XmlReader xr = SetupXmlReaderForTest(sTest);
            AdvanceReaderToTestContent(xr, "Recepients");

            ReadGenericStringElement(xr, sExpectedElement);
            Assert.AreEqual(ntExpected, xr.NodeType);
            if (sNameExpected != null)
                Assert.AreEqual(sNameExpected, xr.Name);
        }

        [TestCase("123", 123, null)]
        [TestCase("-123", -123, null)]
        [TestCase("2147483647", 2147483647, null)]
        [TestCase("-2147483648", -2147483648, null)]
        [TestCase("0", 0, null)]
        [TestCase(null, null, null)]
        [TestCase("2147483648", 0, "System.OverflowException")]
        [TestCase("", 0, "System.FormatException")]
        [TestCase("a", 0, "System.FormatException")]
        [TestCase("1a", 0, "System.FormatException")]
        [Test]
        public static void TestConvertElementStringToInt(string sTest, int? nExpectedVal, string sExpectedException)
        {
            if (sExpectedException == null)
                Assert.AreEqual(nExpectedVal, ConvertElementStringToInt(sTest));
            if (sExpectedException != null)
                RunTestExpectingException(() => ConvertElementStringToInt(sTest), sExpectedException);
        }

        [TestCase("123", 123UL, null)]
        [TestCase("18446744073709551615", 18446744073709551615UL, null)]
        [TestCase("0", 0UL, null)]
        [TestCase(null, null, null)]
        [TestCase("18446744073709551616", 0UL, "System.OverflowException")]
        [TestCase("", 0UL, "System.FormatException")]
        [TestCase("a", 0UL, "System.FormatException")]
        [TestCase("1a", 0UL, "System.FormatException")]
        [Test]
        public static void TestConvertElementStringToUInt64(string sTest, UInt64? nExpectedVal, string sExpectedException)
        {
            if (sExpectedException == null)
                Assert.AreEqual(nExpectedVal, ConvertElementStringToUInt64(sTest));
            if (sExpectedException != null)
                RunTestExpectingException(() => ConvertElementStringToUInt64(sTest), sExpectedException);
        }

        [TestCase("0", false, null)]
        [TestCase("false", false, null)]
        [TestCase("1", true, null)]
        [TestCase("true", true, null)]
        [TestCase(null, null, null)]
        [TestCase("True", false, "System.FormatException")]
        [TestCase("", false, "System.FormatException")]
        [TestCase("2", false, "System.FormatException")]
        [TestCase("1 true", false, "System.FormatException")]
        [Test]
        public static void TestConvertElementStringToBool(string sTest, bool? fExpectedVal, string sExpectedException)
        {
            if (sExpectedException == null)
                Assert.AreEqual(fExpectedVal, ConvertElementStringToBool(sTest));
            if (sExpectedException != null)
                RunTestExpectingException(() => ConvertElementStringToBool(sTest), sExpectedException);
        }

        static string Nullable(string s)
        {
            if (s == "<null>")
                return null;

            return s;
        }

        static SmsMessage SmsCreateFromString(string s)
        {
            string[] rgs = s.Split('|');
            SmsMessage sms = new SmsMessage();

            sms.m_protocol = int.Parse(rgs[0]);
            sms.m_sAddress = Nullable(rgs[1]);
            sms.m_msecUnixDate = UInt64.Parse(rgs[2]);
            sms.m_type = int.Parse(rgs[3]);
            sms.m_sSubject = Nullable(rgs[4]);
            sms.m_sBody = Nullable(rgs[5]);
            sms.m_sToa = Nullable(rgs[6]);
            sms.m_sSc_Toa = Nullable(rgs[7]);
            sms.m_sServiceCenter = Nullable(rgs[8]);
            sms.m_nRead = int.Parse(rgs[9]);
            sms.m_nStatus = int.Parse(rgs[10]);
            sms.m_nLocked = int.Parse(rgs[11]);
            sms.m_sReadableDate = Nullable(rgs[12]);
            sms.m_sContactName = Nullable(rgs[13]);

            return sms;
        }

        // Order is:    nProtocol|sAddress|nUnixDate|nType|sSubject|sBody|sToa|sSc_toa|sServiceCenter|nRead|nStatus|nLocked|nDateSent|sReadableDate|sContactName
        [TestCase(null, "0|<null>|0|0|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Recepients><string>+1234</string></Recepients></Message>", "0|+1234|0|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing</Body></Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><IsIncoming>true</IsIncoming></Message>", "0|<null>|0|1|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><IsRead>1</IsRead></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|1|0|0|<null>|<null>", null)]
        [TestCase("<Message><LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|<null>|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender></Message>", "0|+4321|0|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender><LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender> <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender>\r\n <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender><!-- comment here --> <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Body><![CDATA[testing]]></Body></Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing\nnewline</Body></Message>", "0|<null>|0|2|<null>|testing\nnewline|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Recepients><string>+1234</string></Recepients><Body>foo&amp;bar</Body></Message>", "0|+1234|0|2|<null>|foo&bar|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing</Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|0|0|<null>|<null>", "System.Xml.XmlException")]
        [TestCase("<Message><Recepients><string>+14255551212</string></Recepients><Body>:-)</Body><IsIncoming>false</IsIncoming><IsRead>true</IsRead><Attachments /><LocalTimestamp>131777420698276081</LocalTimestamp><Sender /></Message>", "0|+14255551212|1533268469828|2|<null>|:-)|<null>|<null>|<null>|1|0|0|<null>|<null>", null)]
        [TestCase("<Message></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Unknown>foobar</Unknown></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", "System.Exception")]
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

            XmlReader xr = SetupXmlReaderForTest(sIn);

            try
            {
                AdvanceReaderToTestContent(xr, "Message");
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
                RunTestExpectingException(() => CreateFromWindowsPhoneXmlReader(xr), sExpectedException);
        }

        [Test]
        public static void TestXmlReaderFull()
        {
            string sTest =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Message><Recepients><string>+14254956002</string></Recepients><Body>:-)</Body><IsIncoming>false</IsIncoming><IsRead>true</IsRead><Attachments /><LocalTimestamp>131777420698276081</LocalTimestamp><Sender /></Message></ArrayOfMessage>";

            XmlReader xr = SetupXmlReaderForTest(sTest);
            AdvanceReaderToTestContent(xr, "Message");
        }
        #endregion

    }
}
