


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
            List<SmsMessage> smses;

            using (XmlReader xr = XmlReader.Create(sInFile))
            {
                smses = SmsMessage.ReadMessagesFromWpXml(xr);
                // at this point, we have all the SMS messages ready to write
                xr.Close();
            }

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
