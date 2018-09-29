


using System;
using System.Collections.Generic;
using System.Xml;

namespace wp2droidMsg
{
    public class MmsConverter
    {

        public MmsConverter() { }

        public void Convert(string sInFile, string sOutFile)
        {
            List<WpMessage> mmses;

            using (XmlReader xr = XmlReader.Create(sInFile))
            {
                mmses = WpMessage.ReadMessagesFromXml(xr);

                xr.Close();
            }

            // ok, now we have a collection of messages...now we have to write them out...
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;

            XmlWriter xw = XmlTextWriter.Create(sOutFile, xws);
            
            xw.WriteStartDocument();
            xw.WriteStartElement("smses");
            xw.WriteAttributeString("count", mmses.Count.ToString());
            int seqNo = 1;
            foreach (WpMessage wpm in mmses)
            {
                MmsMessage mms = MmsMessage.CreateFromWpMmsMessage(wpm, seqNo++);
                mms.WriteToDroidXml(xw);
            }

            xw.Flush();
            xw.Close();
            xw.Dispose();
        }
    }
}
