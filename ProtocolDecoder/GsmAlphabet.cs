using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProtocolDecoder
{
    enum DCSEnum { DCS_7_BIT, DCS_8_BIT, DCS_UCS2 };
    class GsmAlphabet
    {

        /**
         * GSM default 7 bit alphabet plus national language locking shift character tables.
         * Comment lines above strings indicate the lower four bits of the table position.
         */
        private const string GsmDefaultAlphabet =
        /* 3GPP TS 23.038 V9.1.1 section 6.2.1 - GSM 7 bit Default Alphabet
         01.....23.....4.....5.....6.....7.....8.....9.....A.B.....C.....D.E.....F.....0.....1 */
        "@\u00a3$\u00a5\u00e8\u00e9\u00f9\u00ec\u00f2\u00c7\n\u00d8\u00f8\r\u00c5\u00e5\u0394_"
            // 2.....3.....4.....5.....6.....7.....8.....9.....A.....B.....C.....D.....E.....
            + "\u03a6\u0393\u039b\u03a9\u03a0\u03a8\u03a3\u0398\u039e\uffff\u00c6\u00e6\u00df"
            // F.....012.34.....56789ABCDEF0123456789ABCDEF0.....123456789ABCDEF0123456789A
            + "\u00c9 !\"#\u00a4%&'()*+,-./0123456789:;<=>?\u00a1ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            // B.....C.....D.....E.....F.....0.....123456789ABCDEF0123456789AB.....C.....D.....
            + "\u00c4\u00d6\u00d1\u00dc\u00a7\u00bfabcdefghijklmnopqrstuvwxyz\u00e4\u00f6\u00f1"
            // E.....F.....
            + "\u00fc\u00e0";

        public static string Decode80Ucs2Text(byte[] bytes)
        {
            string decodedChar;
            StringBuilder decodedAlphaIdentifier = new StringBuilder();

            for (int i = 0; i < (bytes.Length) / 2; i++)
            {
                decodedChar = Convert.ToChar((bytes[2 * i] << 8) + bytes[2 * i + 1]).ToString();
                decodedAlphaIdentifier.Append(decodedChar);
                //TableOutputController.Format((byte[])(bytes.Skip(2 * i).Take(2).ToArray()), "Char: " + decodedChar);
            }

            //如果bytes总长度为奇数，则把最后一个值单独打印出来
            if ((bytes.Length) % 2 != 0)
            {
                //TableOutputController.Format(bytes[bytes.Length - 1], "");
            }
            return decodedAlphaIdentifier.ToString();
        }


        public static string DecodeGSMDefault7BitText(byte[] bytes)
        {
            string decodedChar;
            StringBuilder decodedAlphaIdentifier = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] <= 0x7f)
                {
                    decodedChar = GsmAlphabet.GsmDefaultAlphabet.Substring(bytes[i], 1);
                    decodedAlphaIdentifier.Append(decodedChar);
                    //if (ifDisplay) TableOutputController.Format(bytes[i], "Char: " + decodedChar);
                }
                else
                {
                    return null;// this is not gsm default 7 bit coding
                    //if (ifDisplay) TableOutputController.Format(bytes[i], "");
                }
            }
            return decodedAlphaIdentifier.ToString();
        }

        static byte[] Unpack7BitTo8Bit(byte[] bytes)
        {
            if (bytes == null) return null;

            byte[] result = new byte[(bytes.Length * 8) / 7];
            for (int pos = 0, i = 0, shift = 0; pos < bytes.Length; pos++,i++)
            {
                TableOutputController.Format(bytes[pos], String.Format("Packed 7 bit[{0}]", pos));
                result[i] = (byte)(( bytes[pos] << shift) & 0x7F);
                
                if(pos != 0)
                {
                    result[i] |= (byte)(bytes[pos-1] >> (8-shift));
                }
                
                shift ++;
    
                if( shift == 7 )
                {
                  shift = 0;
                  //if ((pos == (bytes.Length - 1)) && (bytes[pos] >> 1 == 0x0D)) continue;
                  
                  i ++;
                  result[i] = (byte)(bytes[pos] >> 1);

                }
            }
            return result;
        }

        public static string Decode7BitText(byte[] bytes)
        {
            return DecodeGSMDefault7BitText(Unpack7BitTo8Bit(bytes));
        }

        public static void DecodeAlphaIdentifier(byte[] bytes, string name)
        {
            if (bytes.Length == 0)
            {
                return;
            }
            string decodedAlphaIdentifier;
            switch (bytes[0])
            {
                case 0x80:
                    TableOutputController.Format(bytes[0], "UCS2");
                    decodedAlphaIdentifier = GsmAlphabet.Decode80Ucs2Text(bytes.Skip(1).ToArray());
                    break;
                default:
                    decodedAlphaIdentifier = GsmAlphabet.DecodeGSMDefault7BitText(bytes);
                    break;
            }
            TableOutputController.Format(bytes, name, decodedAlphaIdentifier);
            //return decodedAlphaIdentifier;
        }

        public static DCSEnum DecodeDCS(byte dcs, bool isCbs = false)
        {
            if (!isCbs)
            {
                if ((dcs & 0xcc) == 0x04 || (dcs & 0xfc) == 0xf4)
                {
                    Debug.WriteLine("dcs:" + DCSEnum.DCS_8_BIT);
                    return DCSEnum.DCS_8_BIT;
                }
                if ((dcs & 0xcc) == 0x08)
                {
                    Debug.WriteLine("dcs:" + DCSEnum.DCS_UCS2);
                    return DCSEnum.DCS_UCS2;
                }
            }
            else
            {
                if ((dcs & 0xfc) == 0xf4 || (dcs & 0xcc) == 0x44)
                {
                    return DCSEnum.DCS_8_BIT;
                }
                if ((dcs & 0xcc) == 0x48)
                {
                    return DCSEnum.DCS_UCS2;
                }
                if ((dcs & 0x11) == 0x11)
                {
                    return DCSEnum.DCS_UCS2;
                }
            }
            Debug.WriteLine("dcs:" + DCSEnum.DCS_7_BIT);
            return DCSEnum.DCS_7_BIT;
        }


        public static void DecodeTextString(byte[] bytes, DCSEnum alphabet, string name)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            string result = null;
            if (alphabet == DCSEnum.DCS_8_BIT)
            {
                result = DecodeGSMDefault7BitText(bytes);
            }
            else if (alphabet == DCSEnum.DCS_UCS2)
            {
                result = Decode80Ucs2Text(bytes);
            }
            else if (alphabet == DCSEnum.DCS_7_BIT)
            {
                result = Decode7BitText(bytes);
            }
            else
            {
                result = "I don't know how to decode it yet >_<|||";
                //TableOutputController.Format(bytes, "I don't know how to decode it yet >_<|||");
            }

            TableOutputController.Format(bytes, name, result);
        }
    }
}
