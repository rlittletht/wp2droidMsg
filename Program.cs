﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wp2droidMsg
{
    class Program
    {
        static void Main(string[] args)
        {
//            string sInFile = "c:\\temp\\test.xml";
            string sInFile = "c:\\temp\\WpMMS_Test.xml";
            string sOutFile = "c:\\temp\\testOut.xml";

            //SmsConverter smsc = new SmsConverter();

            //smsc.Convert(sInFile, sOutFile);

            MmsConverter smsc = new MmsConverter();

            smsc.Convert(sInFile, sOutFile);

        }
    }
}
