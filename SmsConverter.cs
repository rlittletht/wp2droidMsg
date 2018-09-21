


using System;
using System.Collections.Generic;
using System.Xml;

namespace wp2droidMsg
{
    public class SmsConverter
    {

        public SmsConverter() { }

        public void Convert(string sInFile, string sOutFile)
        {
            XmlReader xr = XmlReader.Create(sInFile);

            if (!xr.Read())
                return;

            List<SmsMessage> smses = new List<SmsMessage>();

            bool fFoundMessageArray = false;
            bool fValidExit = false;

            while (true)
            {
                XmlNodeType nt = xr.NodeType;

                if (nt == XmlNodeType.Element)
                {
                    if (!fFoundMessageArray && xr.Name == "ArrayOfMessage")
                    {
                        xr.ReadStartElement();
                        fFoundMessageArray = true;
                        continue;
                    }

                    // only other valid element is message
                    if (xr.Name != "Message")
                        throw new Exception($"Illegal element {xr.Name} under ArrayOfMessages");

                    smses.Add(SmsMessage.CreateFromWindowsPhoneXmlReader(xr));
                    continue;
                }

                if (nt == XmlNodeType.EndElement)
                {
                    if (xr.Name == "ArrayOfMessage")
                    {
                        fValidExit = true;
                        break; // yay done
                    }

                    throw new Exception($"end element {xr.Name} unexpected");
                }

                if (nt == XmlNodeType.Attribute)
                    throw new Exception($"attribute unexpected per schema");

                // all others are just skipped
                if (!xr.Read())
                    break;
            }

            if (!fValidExit)
                throw new Exception("end of file before end message array");

            // at this point, we have all the SMS messages ready to write
            xr.Close();
            xr.Dispose();

            XmlWriter xw = XmlTextWriter.Create(sOutFile);

            xw.WriteStartDocument();
            xw.WriteStartElement("smses");
            xw.WriteAttributeString("count", smses.Count.ToString());
            foreach (SmsMessage sms in smses)
                sms.WriteToDroidXml(xw);

            xw.Flush();
            xw.Close();
            xw.Dispose();
        }
    }
}
