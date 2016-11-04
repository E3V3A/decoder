using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolDecoder
{

    class CommonDecoder
    {
        // TlvHandler only cares about value part of TLV
        protected delegate void TlvHandler(byte[] bytes);

        public static void HandleLengthError(byte[] bytes)
        {
            if(bytes != null && bytes.Length != 0)
            {
                TableOutputController.Format(bytes, "Invalid format due to length error >_<|||");
            }
        }

        static string BcdByteToString(byte value)
        {
            switch (value)
            {
                case 0xa:
                    return "*";
                case 0xb:
                    return "#";
                case 0xc:
                    return "p";
                case 0xd:
                case 0xe:
                    return Convert.ToChar(value - 0xa + 'a').ToString();
                case 0xf:
                    return "";//这样在调用处就不用判断0xf了
                default:
                    return Convert.ToChar(value + '0').ToString();
            }
        }


        //由于被AddressHandler直接调用，调用处必须自己保证入参数组长度大于0
        //地址是TON/NPI后面跟着BCD号码
        protected static void DecodeAddress(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            Dictionary<byte, string> tonDictionary = new Dictionary<byte, string>
            {
                {0x0, "Unknown Type of Number"},
                {0x1, "International Number"},
                {0x2, "National Number"},
                {0x3, "Network Specific Number"},
            };
            Dictionary<byte, string> npiDictionary = new Dictionary<byte, string>
            {
                {0x0, "Unknown numbering plan"},
                {0x1, "ISDN/telephony numbering plan"},
                {0x3, "Data numbering plan"},
                {0x4, "Telex numbering plan"},
                {0x9, "Private numbering plan"},
                {0xf, "Reserved for extension"},
            };

            List<string> decodedTypeList = new List<string>();

            byte ton = (byte)((bytes[0] & 0x70) >> 4);
            decodedTypeList.Add(GetValueOrDefaultFromDictionary(tonDictionary, ton, "reserved ton value"));

            byte npi = (byte)(bytes[0] & 0x0f);
            decodedTypeList.Add(GetValueOrDefaultFromDictionary(npiDictionary, npi, "reserved npi value"));

            TableOutputController.Format(bytes[0], decodedTypeList);
            DecodeBCDNumber(bytes.Skip(1).ToArray(), "Address");
            //TableOutputController.Format(bytes.Skip(1).ToArray(), "Address: "+DecodeBCDNumber(bytes.Skip(1).ToArray()));
        }

        public static void DecodeBCDNumber(byte[] bytes, string name)
        {
            StringBuilder decodedNumber = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                //TableOutputController.Format(bytes[i], String.Format("BCD number[{0}]", i));
                decodedNumber.Append(BcdByteToString((byte)(bytes[i] & 0x0F)))
                             .Append(BcdByteToString((byte)(bytes[i] >> 4)));
            }
            TableOutputController.Format(bytes, name, decodedNumber.ToString());
            //return decodedNumber.ToString();
        }

        // 参考TS 23.003 9	Definition of Access Point Name 
        public static void DecodeAPN(byte[] bytes, string name)
        {
            StringBuilder apn = new StringBuilder();
            int partLength = 0;
            int i = 0;

            while(i < bytes.Length)
            {
                partLength = bytes[i];
                
                for (int j = (i+1); j <= (i+partLength) & j<bytes.Length; j++)
                {
                    apn.Append(Convert.ToChar(bytes[j]));
                }

                i += (partLength + 1);

                if (i < (bytes.Length - 1))
                {
                    apn.Append(".");
                }
            }
             
            TableOutputController.Format(bytes, name, apn.ToString());
            //return apn.ToString();
        }

        public static void DecodePLMN(byte[] bytes)
        {
            StringBuilder mcc = new StringBuilder();
            StringBuilder mnc = new StringBuilder();
            mcc.Append(Convert.ToString(bytes[0] & 0xf))
               .Append(Convert.ToString(bytes[0] >> 4))
               .Append(Convert.ToString(bytes[1] & 0xf));
            mnc.Append(Convert.ToString(bytes[2] & 0xf))
               .Append(Convert.ToString(bytes[2] >> 4));
            if ((bytes[1] >> 4) != 0xf)
            {
                mnc.Append(Convert.ToString(bytes[1] >> 4));
            }
            TableOutputController.Format(bytes, "MCC=" + mcc.ToString()+", MNC="+mnc.ToString());
        }

        //由调用者保证长度是5的整数倍
        protected static void DecodePLMNwAct(byte[] bytes)
        {
            if (bytes == null || bytes.Length % 5 != 0)
            {
                HandleLengthError(bytes);
                return;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length / 5; i++)
            {
                DecodePLMN(bytes.Skip(5*i).Take(3).ToArray());

                sb.Clear();
                if ((bytes[5 * i + 3] & 0x40) != 0) sb.Append("E-UTRAN ");
                if ((bytes[5 * i + 3] & 0x80) != 0) sb.Append("UTRAN ");
                if ((bytes[5 * i + 4] & 0x80) != 0) sb.Append("GSM ");
                if ((bytes[5 * i + 4] & 0x40) != 0) sb.Append("GSM COMPACT ");
                if ((bytes[5 * i + 4] & 0x20) != 0) sb.Append("cdma2000 HRPD ");
                if ((bytes[5 * i + 4] & 0x10) != 0) sb.Append("cdma2000 1xRTT ");
                TableOutputController.Format(bytes.Skip(5 * i + 3).Take(2).ToArray(), "ACT: " + sb);
            }
        }
        public static string GetValueOrDefaultFromDictionary(Dictionary<byte, string> dictionary, byte key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return value;
            }
            
        }
        public static void DecodeMobileIdentity(byte[] bytes)
        {
            string type;
            switch (bytes[0] & 0x7)
            {
                case 0:
                    type = "No Identity";
                    break;
                case 1:
                    type = "IMSI";
                    break;
                case 2:
                    type = "IMEI";
                    break;
                case 3:
                    type = "IMEISV";
                    break;
                case 4:
                    type = "TMSI";
                    break;
                case 5:
                    type = "TMGI";
                    break;
                default:
                    type = "reserved";
                    break;
            }
            TableOutputController.Format(bytes[0], String.Format("{0}[0]", type));
            StringBuilder sb = new StringBuilder();
            sb.Append(BcdByteToString((byte)(bytes[0] >> 4)));
            for (int i = 1; i < bytes.Length; i++)
            {
                TableOutputController.Format(bytes[i], String.Format("{0}[{1}]", type, i));
                sb.Append(BcdByteToString((byte)(bytes[i] & 0xf)))
                  .Append(BcdByteToString((byte)(bytes[i] >> 4)));
            }
            TableOutputController.Format(type, sb.ToString());
        }

    }
}
