
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace wp2droidMsg
{
    public class XmlIO
    {

        /*----------------------------------------------------------------------------
        	%%Function: StringElementReadFromXml
        	%%Qualified: wp2droidMsg.SmsMessage.StringElementReadFromXml
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static string StringElementReadFromXml(XmlReader xr)
        {
            return xr.ReadElementContentAsString("string", "");
        }


        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericStringElement
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericStringElement
        	%%Contact: rlittle
        	
            read the givent element as a string
        ----------------------------------------------------------------------------*/

        public static string ReadGenericStringElement(XmlReader xr, string sElement)
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

        private static int? ConvertElementStringToInt(string sElementString)
        {
            if (sElementString != null)
                return Int32.Parse(sElementString);

            return null;
        }

        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericIntElement
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericIntElement
        	%%Contact: rlittle
        	
            read the given element as an integer
        ----------------------------------------------------------------------------*/

        private static int? ReadGenericIntElement(XmlReader xr, string sElement)
        {
            return ConvertElementStringToInt(ReadGenericStringElement(xr, sElement));
        }

        /*----------------------------------------------------------------------------
        	%%Function: ConvertElementStringToUInt64
        	%%Qualified: wp2droidMsg.SmsMessage.ConvertElementStringToUInt64
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/

        private static UInt64? ConvertElementStringToUInt64(string sElementString)
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

        public static UInt64? ReadGenericUInt64Element(XmlReader xr, string sElement)
        {
            return ConvertElementStringToUInt64(ReadGenericStringElement(xr, sElement));
        }

        /*----------------------------------------------------------------------------
        	%%Function: ConvertElementStringToBool
        	%%Qualified: wp2droidMsg.SmsMessage.ConvertElementStringToBool
        	%%Contact: rlittle
        	
            convert the given string into a bool
        ----------------------------------------------------------------------------*/

        private static bool? ConvertElementStringToBool(string sElementString)
        {
            if (sElementString == null)
                return null;

            if (sElementString == "0" || sElementString == "false")
                return false;
            if (sElementString == "1" || sElementString == "true")
                return true;

            throw new FormatException($"{sElementString} is not a boolean value");
        }

        /*----------------------------------------------------------------------------
        	%%Function: ReadGenericBoolElement
        	%%Qualified: wp2droidMsg.SmsMessage.ReadGenericBoolElement
        	%%Contact: rlittle
        	
            Read the given element as a boolean
        ----------------------------------------------------------------------------*/

        public static bool? ReadGenericBoolElement(XmlReader xr, string sElement)
        {
            return ConvertElementStringToBool(ReadGenericStringElement(xr, sElement));
        }

        /*----------------------------------------------------------------------------
        	%%Function: RecepientsReadElement
        	%%Qualified: wp2droidMsg.SmsMessage.RecepientsReadElement
        	%%Contact: rlittle
        	
            this is either empty or has a single <string> element as a child
        ----------------------------------------------------------------------------*/
        public static string[] RecepientsReadElement(XmlReader xr)
        {
            if (xr.Name != "Recepients")
                throw new Exception("not at the correct node");

            if (xr.IsEmptyElement)
            {
                xr.ReadStartElement(); // since this is both the start and an empty element
                return null;
            }

            List<string> pls = new List<string>();
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
                    while (true)
                    {
                        string s = XmlIO.StringElementReadFromXml(xr).Trim();
                        pls.Add(s);

                        // now we should be at the EndElement for Recepients
                        if (xr.Name == "Recepients")
                        {
                            xr.ReadEndElement();
                            return pls.ToArray();
                        }

                        if (xr.Name != "string")
                            throw new Exception("not at the correct node");
                    }
                }
            }

            throw new Exception("didn't find string child in recepients");
        }

        #region TESTS
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

        public static void RunTestExpectingException(TestDelegate pfn, string sExpectedException)
        {
            if (sExpectedException == "System.Xml.XmlException")
                Assert.Throws<XmlException>(pfn);
            else if (sExpectedException == "System.Exception")
                Assert.Throws<Exception>(pfn);
            else if (sExpectedException == "System.OverflowException")
                Assert.Throws<OverflowException>(pfn);
            else if (sExpectedException == "System.ArgumentException")
                Assert.Throws<ArgumentException>(pfn);
            else if (sExpectedException == "System.FormatException")
                Assert.Throws<FormatException>(pfn);
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
                Assert.AreEqual(sExpectedReturn, XmlIO.StringElementReadFromXml(xr));
            if (sExpectedException != null)
                RunTestExpectingException(() => XmlIO.StringElementReadFromXml(xr), sExpectedException);
        }

        // NOTE: This does NOT test if the parser is left in a good state!!
        [TestCase("<Recepients><string>+12345</string></Recepients>", new[]{"+12345"}, null)]
        [TestCase("<Recepients><string>\r+12345</string></Recepients>", new[] { "+12345"}, null)]
        [TestCase("<Recepients><string>(111) 222-3333</string></Recepients>", new[] { "(111) 222-3333"}, null)]
        [TestCase("<Recepients>\r\n<string>+12345</string></Recepients>", new[] { "+12345"}, null)]
        [TestCase("<Recepients />", null, null)]
        [TestCase("<Recepients>\r\n\t</Recepients>", null, null)]
        [TestCase("<Recepients></Recepients>", null, null)]
        [TestCase("<Recepients><string2>+12345</string2></Recepients>", null, "System.Xml.XmlException")]
        [TestCase("<Recepients><foo/><string>+12345</string></Recepients>", null, "System.Xml.XmlException")]
        [TestCase("<Recepients><string>+4321</string><string>12345</string></Recepients>", new[] { "+4321", "12345"}, null)]
        [Test]
        public static void TestRecepientsReadElement(string sTest, string []rgsExpectedReturn, string sExpectedException)
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
                Assert.AreEqual(rgsExpectedReturn, RecepientsReadElement(xr));
            if (sExpectedException != null)
                RunTestExpectingException(() => RecepientsReadElement(xr), sExpectedException);
        }

        [TestCase("<bar><Recepients><string>1234</string></Recepients><foo/></bar>", XmlNodeType.Element, "foo")]
        [TestCase("<Recepients><string>1234</string></Recepients> ", XmlNodeType.Whitespace, null)]
        [TestCase("<Recepients><string>1234</string><string>4321</string></Recepients> ", XmlNodeType.Whitespace, null)]
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

        public static string Nullable(string s)
        {
            if (s == "<null>")
                return null;

            return s;
        }

        #endregion
    }
}