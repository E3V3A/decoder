using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolDecoder
{
    class SMSDecoder
    {
        public static void DecodeTime7Byte(byte[] bytes)
        {
            if (bytes.Length != 7)
            {
                CommonDecoder.HandleLengthError(bytes);
                return;
            }
            TableOutputController.Format(bytes[0], "Year");
            TableOutputController.Format(bytes[1], "Month");
            TableOutputController.Format(bytes[2], "Day");
            TableOutputController.Format(bytes[3], "Hour");
            TableOutputController.Format(bytes[4], "Minute");
            TableOutputController.Format(bytes[5], "Second");
            TableOutputController.Format(bytes[6], "Time Zone");
        }
        public static void DecodeTime3Byte(byte[] bytes)
        {
            if (bytes.Length != 3)
            {
                CommonDecoder.HandleLengthError(bytes);
                return;
            }
            TableOutputController.Format(bytes[0], "Hour");
            TableOutputController.Format(bytes[1], "Minute");
            TableOutputController.Format(bytes[2], "Second");
        }
    }
}
