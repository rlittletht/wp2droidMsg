
using System.Xml;

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
    }
}