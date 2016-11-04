using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProtocolDecoder
{
    class CatDecoder : CommonDecoder
    {

        private static void DecodeTlvList(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }

            int index = 0;
            int lengthOfValue = 0;
            int bytesOfLength = 0;

            byte key = (byte)(bytes[0] & 0x7f);
            string tagName = GetValueOrDefaultFromDictionary(CatDataObjDictionary, key, "Unknown");

            TableOutputController.Format(bytes[0], tagName + " tag");
            index++;

            if (bytes.Length >= 2 && bytes[1] != 0x81)
            {
                lengthOfValue = bytes[1];
                bytesOfLength = 1;
                TableOutputController.Format(bytes[1], tagName + " length");
            }
            else if (bytes.Length >= 3 && bytes[1] == 0x81)
            {
                lengthOfValue = bytes[2];
                bytesOfLength = 2;
                TableOutputController.Format(bytes[1], tagName + " length > 127");
                TableOutputController.Format(bytes[2], tagName + " length");
            }
            else
            {
                TableOutputController.Format(bytes.Skip(1).ToArray(), "Invalid TLV format");
                return;
            }
            //index是下一个要处理的字节的索引，同时也是已经解析过的字节总数
            index += bytesOfLength;

            //如果还有数据未解析
            if (index <= (bytes.Length - 1))
            {
                if ((lengthOfValue != 0))
                {
                    //如果剩余字节数多于或这一个tag的value长度，则解析这一个并递归下一个
                    if ((bytes.Length - index) > lengthOfValue)
                    {
                        //在此调用处保证tagHandler的入参byte[]长度大于0
                        byte[] value = bytes.Skip(index).Take(lengthOfValue).ToArray();
                        if (CatHandlerDictionary.ContainsKey(key))
                        {
                            CatHandlerDictionary[key](value);
                        }
                        else
                        {
                            TableOutputController.Format(value, tagName);
                        }
                        index += lengthOfValue;
                        DecodeTlvList(bytes.Skip(index).ToArray());
                    }
                    else
                    {
                        //取剩余所有字节串交给该tag的函数处理
                        //在此调用处保证tagHandler的入参byte[]长度大于0
                        byte[] value = bytes.Skip(index).ToArray();
                        if (CatHandlerDictionary.ContainsKey(key))
                        {
                            CatHandlerDictionary[key](value);
                        }
                        else
                        {
                            TableOutputController.Format(value, tagName);
                        }
                        index += lengthOfValue;
                    }
                }
                //如果该tag的value长度为0，则解析后续内容
                else
                {
                    DecodeTlvList(bytes.Skip(index).ToArray());
                }
            }
            //不再认为这是一种异常，而是做尽力解析
            //else if(lengthOfValue == 0)
            //{
            //    TableOutputController.Format("Left string is shorter than defined length, abort decoding");
            //    return;
            //}

        }

        public static void Decode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }

            byte tag = bytes[0];
            CommandInProcess = 0;

            if (CatTemplatesDictionary.ContainsKey(tag))
            {
                TableOutputController.Format(bytes[0], CatTemplatesDictionary[tag]);

                if (bytes.Length >= 2 && bytes[1] != 0x81)
                {
                    TableOutputController.Format(bytes[1], "Total length");
                    DecodeTlvList(bytes.Skip(2).ToArray());
                }
                else if (bytes.Length >= 3 && bytes[1] == 0x81)
                {
                    TableOutputController.Format(bytes[1], "Total length > 127");
                    TableOutputController.Format(bytes[2], "Total length");
                    DecodeTlvList(bytes.Skip(3).ToArray());
                }
                else
                {
                    HandleLengthError(bytes.Skip(1).ToArray());
                }
                return;
            }
            else if (tag == 0x12)
            {
                TableOutputController.Format(tag, "unknown tag, abort decoding, try removing first byte 12");
            }
            else
            {
                DecodeTlvList(bytes);
            }
        }


        static Dictionary<byte, string> CatTemplatesDictionary = new Dictionary<byte, string>
        {
            {0xD0, "Proactive Command"},
            {0xD1, "SMS-PP Download"},
            {0xD2, "Cell Broadcast Download"},
            {0xD3, "Menu Selection"},
            {0xD4, "Call Control"},
            {0xD5, "MO Short Message control"},
            {0xD6, "Event Download"},
            {0xD7, "Timer Expiration"},
            {0xD9, "USSD Download"},
            {0xDA, "MMS Transfer status"},
            {0xDB, "MMS notification download"},
            {0xDC, "Terminal application"},
            {0xDD, "Geographical Location Reporting"},
            {0xDF, "ProSe Report"},
        };

        static void CommandDetailsHandler(byte[] bytes)
        {
            if (bytes.Length != 3)
            {
                HandleLengthError(bytes);
                return;
            }

            TableOutputController.Format(bytes[0], "Command number");

            CommandInProcess = bytes[1];
            TableOutputController.Format(bytes[1], CatTypeOfCommandDictionary.ContainsKey(CommandInProcess) ? CatTypeOfCommandDictionary[CommandInProcess] : "Unknown type of command");

            byte qualifier = bytes[2];
            Dictionary<byte, string> qualifiersDicionary;
            string decodedQualifier = "reserved command qualifier";

            List<string> decodedQualifierList = new List<string>();

            switch (CommandInProcess)
            {
                case 0x01:
                    qualifiersDicionary = new Dictionary<byte, string> 
                    {
                        {0x00, "NAA Initialization and Full File Change Notification"},
                        {0x01, "File Change Notification"},
                        {0x02, "NAA Initialization and File Change Notification"},
                        {0x03, "NAA Initialization"},
                        {0x04, "UICC Reset"},
                        {0x05, "NAA Application Reset, only applicable for a 3G platform"},
                        {0x06, "NAA Session Reset, only applicable for a 3G platform"},
                        {0x07, "Steering of Roaming REFRESH support"},
                        {0x08, "Steering of Roaming for I-WLAN"},
                    };

                    qualifiersDicionary.TryGetValue(qualifier, out decodedQualifier);
                    TableOutputController.Format(qualifier, decodedQualifier);
                    break;
                case 0x10:
                    qualifiersDicionary = new Dictionary<byte, string>
                    {
                        {0x00, "set up call, but only if not currently busy on another call"},
                        {0x01, "set up call, but only if not currently busy on another call, with redial"},
                        {0x02, "set up call, putting all other calls (if any) on hold"},
                        {0x03, "set up call, putting all other calls (if any) on hold, with redial"},
                        {0x04, "set up call, disconnecting all other calls (if any)"},
                        {0x05, "set up call, disconnecting all other calls (if any), with redial"},
                    };
                    qualifiersDicionary.TryGetValue(qualifier, out decodedQualifier);
                    TableOutputController.Format(qualifier, decodedQualifier);
                    break;
                case 0x13:
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "SMS packing by the terminal required." : "packing not required;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x15:
                    qualifiersDicionary = new Dictionary<byte, string>
                    {
                        {0x00, "launch browser if not already launched"},
                        {0x02, "use the existing browser (the browser shall not use the active existing secured session)"},
                        {0x03, "close the existing browser session and launch new browser session"},
                    };
                    qualifiersDicionary.TryGetValue(qualifier, out decodedQualifier);
                    TableOutputController.Format(qualifier, decodedQualifier);
                    break;
                case 0x20:
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "vibrate alert, if available, with the tone" : "use of vibrate alert is up to the terminal;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x21:
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "high priority" : "normal priority");
                    decodedQualifierList.Add((qualifier & 0x80) == 0x80 ? "clear message after a delay" : "wait for user to clear message");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x22://GET INKEY
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "alphabet set." : "digits (0 to 9, *, # and +) only;");
                    decodedQualifierList.Add((qualifier & 0x02) == 0x02 ? "UCS2 alphabet." : "SMS default alphabet;");
                    decodedQualifierList.Add((qualifier & 0x04) == 0x04 ? "character sets defined by bit 1 and bit 2 are disabled and the \"Yes/No\" response is requested." : "character sets defined by bit 1 and bit 2 are enabled;");
                    decodedQualifierList.Add((qualifier & 0x08) == 0x08 ? "an immediate digit response (0 to 9, * and #) is requested." : "user response shall be displayed. The terminal may allow alteration and/or confirmation;");
                    decodedQualifierList.Add((qualifier & 0x80) == 0x80 ? "help information available." : "no help information available;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x23://GET INPUT
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "alphabet set." : "digits (0 to 9, *, # and +) only;");
                    decodedQualifierList.Add((qualifier & 0x02) == 0x02 ? "UCS2 alphabet." : "SMS default alphabet;");
                    decodedQualifierList.Add((qualifier & 0x04) == 0x04 ? "user input shall not be revealed in any way" : "terminal may echo user input on the display;");
                    decodedQualifierList.Add((qualifier & 0x08) == 0x08 ? "user input to be in SMS packed format." : "user input to be in unpacked format;");
                    decodedQualifierList.Add((qualifier & 0x80) == 0x80 ? "help information available." : "no help information available;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x24://SELECT ITEM
                    if ((qualifier & 0x01) == 0x01)
                    {
                        decodedQualifierList.Add((qualifier & 0x02) == 0x02 ? "presentation as a choice of navigation options if bit 1 is '1'." : "presentation as a choice of data values if bit 1 = '1';");
                    }
                    else
                    {
                        decodedQualifierList.Add("presentation type is not specified;");
                    }
                    decodedQualifierList.Add((qualifier & 0x04) == 0x04 ? "selection using soft key preferred." : "no selection preference;");
                    decodedQualifierList.Add((qualifier & 0x80) == 0x80 ? "help information available." : "no help information available;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x25://SET UP MENU
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "selection using soft key preferred." : "no selection preference;");
                    decodedQualifierList.Add((qualifier & 0x80) == 0x80 ? "help information available." : "no help information available;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x26://PROVIDE LOCAL INFORMATION
                    qualifiersDicionary = new Dictionary<byte, string>
                    {
                        {0x00, "Location Information"},
                        {0x01, "IMEI of the terminal"},
                        {0x02, "Network Measurement results"},
                        {0x03, "Date, time and time zone"},
                        {0x04, "Language setting"},
                        {0x05, "Timing Advance"},
                        {0x06, "Access Technology (single access technology)"},
                        {0x07, "ESN of the terminal"},
                        {0x08, "IMEISV of the terminal"},
                        {0x09, "Search Mode"},
                        {0x0A, "Charge State of the Battery (if class \"g\" is supported)"},
                        {0x0B, "MEID of the terminal"},
                        {0x0C, "current WSID"},
                        {0x0D, "Broadcast Network information according to current Broadcast Network Technology used"},
                        {0x0E, "Multiple Access Technologies"},
                        {0x0F, "Location Information for multiple access technologies"},
                        {0x10, "Network Measurement results for multiple access technologies"},
                        {0x11, "CSG ID list and corresponding HNB name"},
                        {0x12, "H(e)NB IP address"},
                        {0x13, "H(e)NB surrounding macrocells"},
                        {0x14, "current WLAN identifier"},
                    };
                    qualifiersDicionary.TryGetValue(qualifier, out decodedQualifier);
                    TableOutputController.Format(qualifier, decodedQualifier);
                    break;
                case 0x27://TIMER MANAGEMENT
                    qualifiersDicionary = new Dictionary<byte, string>
                    {
                        {0x00, "start;"},
                        {0x01, "deactivate;"},
                        {0x02, "get current value;"},
                    };
                    qualifiersDicionary.TryGetValue((byte)(qualifier & 0x3), out decodedQualifier);
                    TableOutputController.Format(qualifier, decodedQualifier);
                    break;
                case 0x35://LANGUAGE NOTIFICATION
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "specific language notification." : "non-specific language notification;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x40://OPEN CHANNEL
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "immediate link establishment." : "on demand link establishment;");
                    if ((qualifier & 0x02) == 0x02) { decodedQualifierList.Add("automatic reconnection."); }
                    if ((qualifier & 0x02) == 0x04) { decodedQualifierList.Add("immediate link establishment in background mode (bit 1 is ignored)."); }
                    if ((qualifier & 0x02) == 0x08) { decodedQualifierList.Add("DNS server address(es) requested (for packet data service only)."); }

                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x41://CLOSE CHANNEL
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "indication to terminal that next CAT command will be OPEN CHANNEL using same Network Access Name and Bearer Description as channel to be closed." : "no indication;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                case 0x43://SEND DATA
                    decodedQualifierList.Add((qualifier & 0x01) == 0x01 ? "send data immediately." : "store data in Tx buffer;");
                    TableOutputController.Format(qualifier, decodedQualifierList);
                    break;
                default:
                    TableOutputController.Format(qualifier, decodedQualifier);
                    break;
            }
        }

        static void DeviceIdentityHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                TableOutputController.Format(bytes, "Invalid device identify");
                return;
            }

            Dictionary<byte, string> deviceIdentitiesDicionary = new Dictionary<byte, string>
            {
                {0x01, "Keypad"},
                {0x02, "Display"},
                {0x03, "Earpiece"},
                {0x81, "UICC"},
                {0x82, "Terminal"},
                {0x83, "Network"},
            };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                sb.Clear();

                sb.Append(i == 0 ? "Source device -> " : "Destination device -> ");

                if (deviceIdentitiesDicionary.ContainsKey(bytes[i]))
                {
                    sb.Append(deviceIdentitiesDicionary[bytes[i]]);
                }
                else if (bytes[i] >= 0x10 && bytes[i] <= 0x17)
                {
                    sb.Append("Additional Card Reader " + (bytes[i] & 0x0f));
                }
                else if (bytes[i] >= 0x21 && bytes[i] <= 0x27)
                {
                    sb.Append("Channel with Channel identifier " + (bytes[i] & 0x0f));
                }
                else if (bytes[i] >= 0x31 && bytes[i] <= 0x37)
                {
                    sb.Append("eCAT client identifier " + (bytes[i] & 0x0f));
                }
                else
                {
                    sb.Append("reserved value");
                }

                TableOutputController.Format(bytes[i], sb.ToString());
            }

        }

        static void AlphaIdentifierHandler(byte[] bytes)
        {
            GsmAlphabet.DecodeAlphaIdentifier(bytes, "Alpha identifier");
            //TableOutputController.Format("Alpha identifier", GsmAlphabet.DecodeAlphaIdentifier(bytes));
        }

        static void DTMFStringHandler(byte[] bytes)
        {
            DecodeBCDNumber(bytes, "DTMF String");
            //TableOutputController.Format("DTMF String", DecodeBCDNumber(bytes));

        }

        static void AddressHandler(byte[] bytes)
        {
            DecodeAddress(bytes);
        }

        static string GetSemiOctets(byte value)
        {
            return String.Format("{0:X}{1:X}", value & 0x0f, ((value & 0xf0) >> 4));
        }

        static void DecodeServiceCenterTimeStamp(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            if (bytes.Length != 7)
            {
                return;
            }
            /*TableOutputController.Format(bytes[0], "Year");
            TableOutputController.Format(bytes[1], "Month");
            TableOutputController.Format(bytes[2], "Day");
            TableOutputController.Format(bytes[3], "Hour");
            TableOutputController.Format(bytes[4], "Minute");
            TableOutputController.Format(bytes[5], "Second");
            TableOutputController.Format(bytes[6], "Time Zone");*/

            string sign = ((bytes[6]&0x08)==0x08)?"-":"+";
            int timeDiffValue = ((((bytes[6] & 0x07) << 4) + (bytes[6] >> 4)));//15分钟的整数倍
            
            string result = String.Format("{0}-{1}-{2} {3}:{4}:{5} GMT{6}{7}:{8:X2}", 
                GetSemiOctets(bytes[0]), GetSemiOctets(bytes[1]), GetSemiOctets(bytes[2]),
                GetSemiOctets(bytes[3]), GetSemiOctets(bytes[4]), GetSemiOctets(bytes[5]),
                sign, (timeDiffValue / 4), 15*(timeDiffValue % 4));
            TableOutputController.Format(bytes, "TP Service Centre Time Stamp", result);
        }

        //不包含user data length
        static void DecodeUserData(byte[] bytes, int udhi, DCSEnum alphabet)
        {
            if (bytes.Length == 0)
            {
                return;
            }
            int index = 0;
            if (udhi == 1)
            {
                byte headerLength = bytes[0];
                TableOutputController.Format(bytes[0], "TP User Data Header Length");
                index++;

                if (bytes.Length >= (index + headerLength))
                {
                    TableOutputController.Format(bytes.Skip(index).Take(headerLength).ToArray(), "TP User Data Header");
                    index += headerLength;
                }
                else
                {
                    TableOutputController.Format(bytes.Skip(index).ToArray(), "TP User Data Header");
                    return;
                }
            }
            GsmAlphabet.DecodeTextString(bytes.Skip(index).ToArray(), alphabet, "TP User Data");
            //TableOutputController.Format("TP User Data", GsmAlphabet.DecodeTextString(bytes.Skip(index).ToArray(), alphabet));
        }

        //参考TS 23.040，暂时假设都是SMS Submit一种消息类型，对长度进行更严格的限制，以简化逻辑
        static void SmsTpduHandler(byte[] bytes)
        {
            List<string> decodedHeader = new List<string>();
            int index = 0;
            int vpf = 0;
            int mti = 0;
            DCSEnum alphabet = 0;
            int udhi = 0;

            Dictionary<byte, string> mtiDictionary = new Dictionary<byte, string>
            {
                {0x0, "TP Message Type Indicator: SMS DELIVER"},//应该只用于SMS PP DOWNLOAD，都是从SC到MS，所以不会是SMS DELIVER REPORT
                {0x1, "TP Message Type Indicator: SMS SUBMIT"},//应该都是SEND SMS中的消息单元，所以一定是MS到SC的SMS SUBMIT，而不是反过来的SMS SUBMIT REPORT
                {0x2, "TP Message Type Indicator: SMS COMMAND"},
                {0x3, "TP Message Type Indicator: reserved"}
            };

            Dictionary<byte, string> piDictionary = new Dictionary<byte, string>
            {
                {0x40, "TP Protocol Identifier: Short Message Type 0"},
                {0x7d, "TP Protocol Identifier: ME Data download"},
                {0x7e, "TP Protocol Identifier: ME De personalization Short Message"},
                {0x7f, "TP Protocol Identifier: (U)SIM Data download"},
            };

            Dictionary<byte, string> vpfDictionary = new Dictionary<byte, string>
            {
                {0x0, "TP Validity Period Format: TP VP field not present"},
                {0x1, "TP Validity Period Format: TP VP field present - relative format"},
                {0x2, "TP Validity Period Format: TP VP field present - enhanced format"},
                {0x3, "TP Validity Period Format: TP VP field present - absolute format"}
            };

            mti = (bytes[0] & 0x3);
            decodedHeader.Add(mtiDictionary[(byte)(bytes[0] & 0x3)]);

            //9.2.2.1	SMS DELIVER type
            if (mti == 0)
            {
                decodedHeader.Add("TP More Messages to Send: " + ((bytes[0] & 0x04) >> 2));
                decodedHeader.Add("TP Status Report Indication: " + ((bytes[0] & 0x20) >> 5));
                udhi = ((bytes[0] & 0x40) >> 6);
                decodedHeader.Add("TP User Data Header Indicator: " + udhi);
                decodedHeader.Add("TP Reply Path: " + ((bytes[0] & 0x80) >> 7));
                TableOutputController.Format(bytes[0], decodedHeader);
                index++;

                if (index <= (bytes.Length - 1))
                {
                    //9.2.3.7	TP Originating Address (TP OA)
                    TableOutputController.Format(bytes[1], "TP Originating Address Length");
                    index++;

                    //包括TON/NPI和地址
                    int addressBytes = (bytes[1] + 1) / 2 + 1;
                    if (bytes.Length >= (index + addressBytes))
                    {
                        DecodeAddress(bytes.Skip(index).Take(addressBytes).ToArray());
                        index += addressBytes;

                        if (bytes.Length >= (index + 10))
                        {
                            TableOutputController.Format(bytes[index], GetValueOrDefaultFromDictionary(piDictionary, bytes[index], "TP Protocol Identifier"));
                            index++;

                            alphabet = GsmAlphabet.DecodeDCS(bytes[index]);
                            TableOutputController.Format(bytes[index], "TP Data Coding Scheme");
                            index++;

                            DecodeServiceCenterTimeStamp(bytes.Skip(index).Take(7).ToArray());
                            //TableOutputController.Format("TP Service Centre Time Stamp", DecodeServiceCenterTimeStamp(bytes.Skip(index).Take(7).ToArray()));
                            index += 7;

                            TableOutputController.Format(bytes[index], "TP User Data Length");
                            index++;

                            DecodeUserData(bytes.Skip(index).ToArray(), udhi, alphabet);
                        }
                    }
                }
            }
            else if (mti == 0x1)
            {
                decodedHeader.Add("TP‑Reject‑Duplicates: " + ((bytes[0] & 0x4) >> 2));

                vpf = ((bytes[0] & 0x18) >> 3);
                decodedHeader.Add(vpfDictionary[(byte)vpf]);
                decodedHeader.Add("TP Reply Path: " + ((bytes[0] & 0x20) >> 5));
                udhi = ((bytes[0] & 0x40) >> 6);
                decodedHeader.Add("TP User Data Header Indicator: " + udhi);
                decodedHeader.Add("TP Status Report Request: " + ((bytes[0] & 0x80) >> 7));

                TableOutputController.Format(bytes[0], decodedHeader);
                index++;

                if (bytes.Length >= (index + 3))
                {
                    TableOutputController.Format(bytes[1], "TP Message Reference");
                    index++;

                    TableOutputController.Format(bytes[2], "TP Destination Address Length");
                    index++;

                    int addressBytes = (bytes[2] + 1) / 2 + 1;//包括TON/NPI和地址

                    if (bytes.Length >= (index + addressBytes))
                    {
                        DecodeAddress(bytes.Skip(index).Take(addressBytes).ToArray());
                        index += addressBytes;

                        if (bytes.Length >= (index + 2))
                        {
                            TableOutputController.Format(bytes[index], GetValueOrDefaultFromDictionary(piDictionary, bytes[index], "TP Protocol Identifier"));
                            index++;

                            alphabet = GsmAlphabet.DecodeDCS(bytes[index]);
                            TableOutputController.Format(bytes[index], "TP Data Coding Scheme");
                            index++;

                            if (vpf == 1 && bytes.Length >= (index + 1))
                            {
                                TableOutputController.Format(bytes[index], "TP Validity Period");
                                index++;
                            }
                            else if ((vpf == 2 || vpf == 3) && bytes.Length >= (index + 7))
                            {
                                TableOutputController.Format(bytes.Skip(index).Take(7).ToArray(), "TP Validity Period");
                                index += 7;
                            }

                            if (bytes.Length >= (index + 1))
                            {
                                int udl = bytes[index];
                                TableOutputController.Format(bytes[index], "TP User Data Length");
                                index++;

                                DecodeUserData(bytes.Skip(index).ToArray(), udhi, alphabet);
                            }
                        }
                    }
                }
            }
            else
            {
                TableOutputController.Format(bytes, "SMS TPDU");
            }

            //暂时假定了mti总是SMS Submit，UDHI长度为0，如果出现意外情况，报了这个错误，再扩展功能，而不是修改这里的错误提示
            //if (bytes.Length > (index + 1))
            //{
            //    HandleLengthError(bytes.Skip(index).Take(bytes.Length - index - 1).ToArray());
            //}

        }

        static void ItemHandler(byte[] bytes)
        {
            TableOutputController.Format(bytes[0], "Identifier of item");
            GsmAlphabet.DecodeAlphaIdentifier(bytes.Skip(1).ToArray(), "Text string of item");
            //TableOutputController.Format("Text string of item", GsmAlphabet.DecodeAlphaIdentifier(bytes.Skip(1).ToArray()));
        }

        static void TextStringHandler(byte[] bytes)
        {
            DCSEnum alphabet = GsmAlphabet.DecodeDCS(bytes[0]);
            TableOutputController.Format(bytes[0], "Data Coding Scheme");

            if (bytes.Length >= 2)
            {
                GsmAlphabet.DecodeTextString(bytes.Skip(1).ToArray(), alphabet, "Text string");
                //TableOutputController.Format("Text string", GsmAlphabet.DecodeTextString(bytes.Skip(1).ToArray(), alphabet));
            }
        }

        static void BearerDescriptionHandler(byte[] bytes)
        {
            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
            {
                {0x01, "CSD"},
                {0x02, "GPRS / UTRAN packet service / E-UTRAN"},
                {0x03, "default bearer for requested transport layer"},
                {0x04, "local link technology independent"},
                {0x05, "Bluetooth"},
                {0x06, "IrDA"},
                {0x07, "RS232"},
                {0x08, "cdma2000 packet data service [57]"},
                {0x09, "UTRAN packet service with extended parameters / HSDPA / E-UTRAN"},
                {0x0A, "(I-)WLAN"},
                {0x0B, "E-UTRAN / mapped UTRAN packet service"},
                {0x10, "USB"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(tempDictionary, bytes[0], "reserved value"));
            if (bytes.Length == 7 && bytes[0] == 2)
            {
                TableOutputController.Format(bytes[1], "Precedence class");
                TableOutputController.Format(bytes[2], "Delay class");
                TableOutputController.Format(bytes[3], "Reliability  class");
                TableOutputController.Format(bytes[4], "Peak throughput class");
                TableOutputController.Format(bytes[5], "Mean throughput class");
                TableOutputController.Format(bytes[6], "Packet data protocol class: " + ((bytes[6] == 0x02) ? "IP" : "reserved value"));
            }
            else
            {
                TableOutputController.Format(bytes.Skip(1).ToArray(), "Bearer parameters", false);
            }
        }

        static void URLHandler(byte[] bytes)
        {
            GsmAlphabet.DecodeTextString(bytes, DCSEnum.DCS_8_BIT, "URL");
            //TableOutputController.Format("URL", GsmAlphabet.DecodeTextString(bytes, DCSEnum.DCS_8_BIT));
        }

        static void ChannelDataLengthHandler(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                HandleLengthError(bytes);
                return;
            }
            if (bytes[0] == 0xff)
            {
                TableOutputController.Format(bytes[0], "More than 255 bytes of space available in buffer");
            }
            else
            {
                TableOutputController.Format(bytes[0], "Channel data length: " + bytes[0]);
            }
        }

        static void IconIdentifierHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                TableOutputController.Format(bytes, "invalid icon identifier");
            }
            TableOutputController.Format(bytes[0], "Icon qualifier");
            TableOutputController.Format(bytes[1], "Icon identifier");
        }
        static void TextAttributeHandler(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
            {
                HandleLengthError(bytes);
                return;
            }
            for (int i = 0; i < bytes.Length / 4; i++)
            {
                TableOutputController.Format(bytes[4 * i], "Formatting position");
                TableOutputController.Format(bytes[4 * i + 1], "Formatting length");
                TableOutputController.Format(bytes[4 * i + 2], "Formatting mode:");
                TableOutputController.Format(bytes[4 * i + 3], "Text colour");
            }
        }

        static void ResponseLengthHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                HandleLengthError(bytes);
                return;
            }
            TableOutputController.Format(bytes[0], "Minimum length of response");
            TableOutputController.Format(bytes[1], "Maximum length of response");
        }



        static void ResultHandler(byte[] bytes)
        {
            int index = 0;
            if (ResultDictionary.ContainsKey(bytes[0]))
            {
                TableOutputController.Format(bytes[0], ResultDictionary[bytes[0]]);
                index++;
            }
            else
            {
                TableOutputController.Format(bytes[0], "reserved value");
                TableOutputController.Format(bytes.Skip(1).ToArray(), "abort additional info since result value is meaningless");
                return;
            }
            if (bytes.Length == 1)
            {
                return;
            }
            Dictionary<byte, string> tempDictionary;
            switch (bytes[0])
            {
                case 0x00:
                    if (CommandInProcess == 0x11)
                    {
                        TableOutputController.Format(bytes[1], "SS Return Result Operation code");
                        index++;

                        TableOutputController.Format(bytes.Skip(index).ToArray(), "SS Return Result Parameters", false);

                        return;
                    }
                    break;
                case 0x20://terminal currently unable to process command
                    tempDictionary = new Dictionary<byte, string>
                    {
                        {0x00, "No specific cause can be given"},
                        {0x01, "Screen is busy"},
                        {0x02, "terminal currently busy on call"},
                        {0x03, "ME currently busy on SS transaction"},
                        {0x04, "No service"},
                        {0x05, "Access control class bar"},
                        {0x06, "Radio resource not granted"},
                        {0x07, "Not in speech call"},
                        {0x08, "ME currently busy on USSD transaction"},
                        {0x09, "terminal currently busy on SEND DTMF command"},
                        {0x0A, "No NAA active"},
                    };

                    TableOutputController.Format(bytes[1], GetValueOrDefaultFromDictionary(tempDictionary, bytes[1], tempDictionary[0]));
                    index++;
                    break;
                case 0x21:
                    TableOutputController.Format(bytes[1], "No specific cause can be given");
                    index++;
                    break;
                case 0x25:
                    tempDictionary = new Dictionary<byte, string>
                    {
                        {0x00, "No specific cause can be given;"},
                        {0x01, "Action not allowed;"},
                        {0x02, "The type of request has changed."},
                    };
                    TableOutputController.Format(bytes[1], GetValueOrDefaultFromDictionary(tempDictionary, bytes[1], tempDictionary[0]));
                    index++;
                    break;
                case 0x26:
                    tempDictionary = new Dictionary<byte, string>
                    {
                        {0x00, "No specific cause can be given"},
                        {0x01, "Bearer unavailable"},
                        {0x02, "Browser unavailable"},
                        {0x03, "terminal unable to read the provisioning data"},
                        {0x04, "Default URL unavailable"},
                    };
                    TableOutputController.Format(bytes[1], GetValueOrDefaultFromDictionary(tempDictionary, bytes[1], tempDictionary[0]));
                    index++;
                    break;
                case 0x34:
                case 0x37:
                    if (bytes[1] == 0)
                    {
                        TableOutputController.Format(bytes[1], "No specific cause can be given");
                    }
                    else
                    {
                        TableOutputController.Format(bytes[1], "Additional information: " + GetValueOrDefaultFromDictionary(SupsErrorCodeDictionary, bytes[1], "unknown error code"));
                    }
                    index++;
                    break;
                case 0x38:
                    tempDictionary = new Dictionary<byte, string>
                    {
                        {0x00, "No specific cause can be given"},
                        {0x01, "Card reader removed or not present"},
                        {0x02, "Card removed or not present"},
                        {0x03, "Card reader busy"},
                        {0x04, "Card powered off"},
                        {0x05, "C-APDU format error"},
                        {0x06, "Mute card"},
                        {0x07, "Transmission error"},
                        {0x08, "Protocol not supported"},
                        {0x09, "Specified reader not valid"},    
                    };
                    TableOutputController.Format(bytes[1], GetValueOrDefaultFromDictionary(tempDictionary, bytes[1], tempDictionary[0]));
                    index++;
                    break;
                case 0x3a:
                    tempDictionary = new Dictionary<byte, string>
                    {
                        {0x00, "No specific cause can be given"},
                        {0x01, "No channel available"},
                        {0x02, "Channel closed"},
                        {0x03, "Channel identifier not valid"},
                        {0x04, "Requested buffer size not available"},
                        {0x05, "Security error (unsuccessful authentication)"},
                        {0x06, "Requested UICC/terminal interface transport level not available"},
                        {0x07, "remote device is not reachable"},
                        {0x08, "Service error"},
                        {0x09, "Service identifier unknown"},
                        {0x10, "Port not available and Terminal Server Mode"},
                        {0x11, "Launch parameters missing or incorrect"},
                        {0x12, "Application launch failed"},
                    };
                    TableOutputController.Format(bytes[1], GetValueOrDefaultFromDictionary(tempDictionary, bytes[1], tempDictionary[0]));
                    index++;
                    break;
            }

            //和短信类似，如果由于能力不足而报这个错误，应该考虑补齐解析能力，而不是修改报错
            if (index <= (bytes.Length - 1))
            {
                HandleLengthError(bytes.Skip(index).ToArray());
                return;
            }
        }
        static void ToneHandler(byte[] bytes)
        {
            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
            {
                {0x01, "Dial tone"},
                {0x02, "Called subscriber busy"},
                {0x03, "Congestion"},
                {0x04, "Radio path acknowledge"},
                {0x05, "Radio path not available/Call dropped"},
                {0x06, "Error/Special information"},
                {0x07, "Call waiting tone"},
                {0x08, "Ringing tone"},
                {0x10, "General beep"},
                {0x11, "Positive acknowledgement tone"},
                {0x12, "Negative acknowledgement or error tone"},
                {0x13, "Ringing tone as selected by the user for incoming speech call"},
                {0x14, "Alert tone as selected by the user for incoming SMS"},
                {0x15, "Critical Alert"},
                {0x20, "vibrate only, if available"},
                {0x30, "happy tone"},
                {0x31, "sad tone"},
                {0x32, "urgent action tone"},
                {0x33, "question tone"},
                {0x34, "message received tone"},
                {0x40, "Melody 1"},
                {0x41, "Melody 2"},
                {0x42, "Melody 3"},
                {0x43, "Melody 4"},
                {0x44, "Melody 5"},
                {0x45, "Melody 6"},
                {0x46, "Melody 7"},
                {0x47, "Melody 8"},
            };


            if (bytes.Length != 1)
            {
                HandleLengthError(bytes);
                return;
            }

            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(tempDictionary, bytes[0], "reserved value"));
        }

        static void DurationHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                HandleLengthError(bytes);
                return;
            }

            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
                    {
                        {0x00, "Time unit: minutes"},
                        {0x01, "Time unit: seconds"},
                        {0x02, "Time unit: tenths of seconds"},
                    };
            TableOutputController.Format(bytes[0], tempDictionary.ContainsKey(bytes[0]) ? tempDictionary[bytes[0]] : "Time unit reserved");
            TableOutputController.Format(bytes[1], String.Format("Time interval: {0} unit(s)", bytes[1]));
        }
        static void FileListHandler(byte[] bytes)
        {
            TableOutputController.Format(bytes[0], "Number Of Files");
            TableOutputController.Format(bytes.Skip(1).ToArray(), "File Path");

        }
        static void ItemIdentifierHandler(byte[] bytes)
        {
            TableOutputController.Format(bytes[0], "Identifier of item chosen");
        }
        static void PLMNwAcTListHandler(byte[] bytes)
        {
            DecodePLMNwAct(bytes);
        }
        static void DateTimeAndTimezoneHandler(byte[] bytes)
        {
            SMS.DecodeTime7Byte(bytes);
        }
        static void LanguageHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                HandleLengthError(bytes);
                return;
            }
            TableOutputController.Format("Language is ", GsmAlphabet.DecodeGSMDefault7BitText(bytes));

        }

        static void EventListHandler(byte[] bytes)
        {
            Dictionary<byte, string> eventDictionary = new Dictionary<byte, string>
            {
                {0x00, "MT call"},
                {0x01, "Call connected"},
                {0x02, "Call disconnected"},
                {0x03, "Location status"},
                {0x04, "User activity"},
                {0x05, "Idle screen available"},
                {0x06, "Card reader status"},
                {0x07, "Language selection"},
                {0x08, "Browser termination"},
                {0x09, "Data available"},
                {0x0A, "Channel status"},
                {0x0B, "Access Technology Change (single access technology)"},
                {0x0C, "Display parameters changed"},
                {0x0D, "Local connection"},
                {0x0E, "Network Search Mode Change"},
                {0x0F, "Browsing status"},
                {0x10, "Frames Information Change"},
                {0x11, "I-WLAN Access Status"},
                {0x12, "Network Rejection"},
                {0x13, "HCI connectivity event"},
                {0x14, "Access Technology Change (multiple access technologies)"},
                {0x15, "CSG Cell selection"},
                {0x16, "Contactless state request"},
                {0x17, "IMS Registration"},
                {0x18, "IMS Incoming data"},
                {0x19, "Profile Container"},
                {0x1A, "Void"},
                {0x1B, "Secured Profile Container"},
                {0x1C, "Poll Interval Negotiation"},
            };
            for (int i = 0; i < bytes.Length; i++)
            {
                TableOutputController.Format(bytes[i], GetValueOrDefaultFromDictionary(eventDictionary, bytes[i], "unknown"));
            }
        }
        static void TimerIdentifierHandler(byte[] bytes)
        {
            TableOutputController.Format(bytes[0], "Timer identifier");
        }
        static void TimerValueHandler(byte[] bytes)
        {
            if (bytes.Length != 3)
            {
                HandleLengthError(bytes);
                return;
            }
            SMS.DecodeTime3Byte(bytes);
        }


        static void BufferSizeHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                HandleLengthError(bytes);
                return;
            }
            TableOutputController.Format(bytes, "Buffer size: " + ((bytes[0] << 8) | bytes[1]));
        }
        static void UICCTerminalInterfaceTransportLevelHandler(byte[] bytes)
        {
            if (bytes.Length != 3)
            {
                HandleLengthError(bytes);
                return;
            }
            Dictionary<byte, string> protocolDictionary = new Dictionary<byte, string>
            {
                {0x01, "UDP, UICC in client mode, remote connection"},
                {0x02, "TCP, UICC in client mode, remote connection"},
                {0x03, "TCP, UICC in server mode"},
                {0x04, "UDP, UICC in client mode, local connection"},
                {0x05, "TCP, UICC in client mode, local connection"},
                {0x06, "direct communication channel"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(protocolDictionary, bytes[0], "Transport protocol type reserved"));
            TableOutputController.Format(bytes.Skip(1).ToArray(), "Port number: " + ((bytes[1] << 8) | bytes[2]));
        }
        static void ChannelStatusHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                HandleLengthError(bytes);
                return;
            }
            TableOutputController.Format(bytes, "Channel status");
        }

        static void SSStringHandler(byte[] bytes)
        {
            DecodeAddress(bytes);
        }

        static void USSDStringHandler(byte[] bytes)
        {
            DCSEnum alphabet = GsmAlphabet.DecodeDCS(bytes[0], true);
            TableOutputController.Format(bytes[0], "Data coding scheme");
            GsmAlphabet.DecodeTextString(bytes.Skip(1).ToArray(), alphabet, "USSD string");
            //TableOutputController.Format("USSD string", GsmAlphabet.DecodeTextString(bytes.Skip(1).ToArray(), alphabet));
        }

        static void LocationInformationHandler(byte[] bytes)
        {
            int index = 0;

            //长度为5，7，9的算作umts的location info,其它的算作cdma的。
            if (bytes.Length != 5 & bytes.Length != 7 && bytes.Length != 9 && bytes.Length >= 2)
            {
                TableOutputController.Format(bytes.Take(2).ToArray(), "CDMA MCC");
                index += 2;
                if (bytes.Length >= 3)
                {
                    TableOutputController.Format(bytes.Skip(2).Take(1).ToArray(), "IMSI 11_12");
                    index += 1;
                    if (bytes.Length >= 5)
                    {
                        TableOutputController.Format(bytes.Skip(3).Take(2).ToArray(), "SID");
                        index += 2;
                        if (bytes.Length >= 7)
                        {
                            TableOutputController.Format(bytes.Skip(5).Take(2).ToArray(), "NID");
                            index += 2;
                            if (bytes.Length >= 9)
                            {
                                TableOutputController.Format(bytes.Skip(7).Take(2).ToArray(), "BASE ID");
                                index += 2;
                                if (bytes.Length >= 12)
                                {
                                    TableOutputController.Format(bytes.Skip(9).Take(3).ToArray(), "BASE LAT");
                                    index += 3;
                                    if (bytes.Length >= 15)
                                    {
                                        TableOutputController.Format(bytes.Skip(12).Take(3).ToArray(), "BASE LONG");
                                        index += 3;
                                    }
                                }
                            }
                        }
                    }
                }

            }

            //否则就是gsm的Location information
            else if (bytes.Length >= 5)
            {
                DecodePLMN(bytes.Take(3).ToArray());
                TableOutputController.Format(bytes.Skip(3).Take(2).ToArray(), "Location Area Code");
                index += 5;

                if (bytes.Length >= 7)
                {
                    TableOutputController.Format(bytes.Skip(5).Take(2).ToArray(), "Cell Identity Value");
                    index += 2;

                    if (bytes.Length == 9)
                    {
                        TableOutputController.Format(bytes.Skip(7).Take(2).ToArray(), "Extended Cell Identity Value");
                        index += 2;
                    }
                }
            }

            if (index <= (bytes.Length - 1))
            {
                HandleLengthError(bytes.Skip(index).ToArray());
                return;
            }
        }

        static void IMEIHandler(byte[] bytes)
        {
            DecodeMobileIdentity(bytes);
        }

        static void IMEISVHandler(byte[] bytes)
        {
            DecodeMobileIdentity(bytes);
        }

        static void TimingAdvanceHandler(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                HandleLengthError(bytes);
                return;
            }
            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
            {
                {0x0, "ME is in the idle state"},
                {0x1, "ME is not in idle state"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(tempDictionary, bytes[0], "reserved values"));
            TableOutputController.Format(bytes[1], "Timing Advance");
        }

        static void NetworkSearchModeHandler(byte[] bytes)
        {
            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
            {
                {0x0, "Manual"},
                {0x1, "Automatic"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(tempDictionary, bytes[0], "reserved values"));
        }

        static void AccessTechnologyHandler(byte[] bytes)
        {
            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
            {
                {0x00, "GSM"},
                {0x01, "TIA/EIA-553 [49]"},
                {0x02, "TIA/EIA-136-C [25]"},
                {0x03, "UTRAN"},
                {0x04, "TETRA"},
                {0x05, "TIA/EIA-95 [50]"},
                {0x06, "cdma2000 1x (TIA/EIA/IS-2000 [51])"},
                {0x07, "cdma2000 HRPD (TIA/EIA/IS-856 [52])"},
                {0x08, "E-UTRAN"},
                {0x09, "eHRPD [54]"},
            };
            for (int i = 0; i < bytes.Length; i++)
            {
                TableOutputController.Format(bytes[i], GetValueOrDefaultFromDictionary(tempDictionary, bytes[i], "reserved value"));
            }
        }

        static void DecodeIPAddress(byte[] bytes, string name)
        {
            if (bytes.Length == 4)
            {
                TableOutputController.Format(bytes, name, String.Format("{0}.{1}.{2}.{3}", bytes[0], bytes[1], bytes[2], bytes[3]));
            }
            else
            {
                TableOutputController.Format(bytes, name);
            }
        }

        static void OtherAddressHandler(byte[] bytes)
        {
            Dictionary<byte, string> tempDictionary = new Dictionary<byte, string>
            {
                {0x21, "Ipv4 address"},
                {0x57, "Ipv6 address"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(tempDictionary, bytes[0], "reserved type of address"));
            DecodeIPAddress(bytes.Skip(1).ToArray(), "Data destination address");
            //TableOutputController.Format(bytes.Skip(1).ToArray(), "Data destination address", false);
        }


        static void NetworkAccessNameHandler(byte[] bytes)
        {
            DecodeAPN(bytes, "Network Access Name");
            //TableOutputController.Format(bytes, DecodeAPN(bytes), "Network Access Name");
        }

        //参考TS23.041 9.4.1	GSM
        static void CellBroadcastPageHandler(byte[] bytes)
        {
            if (bytes.Length != 88)
            {
                HandleLengthError(bytes);
                return;
            }

            List<string> list = new List<string>();
            Dictionary<byte, string> dictionary = new Dictionary<byte, string>
            {
                {0x0, "Cell wide, immediate display mode"},
                {0x1, "PLMN wide, normal display mode"},
                {0x2, "Location area wide, normal display mode"},
                {0x3, "Cell wide, normal display mode"},
            };
            list.Add("Serial Number: Geographical scope: " + dictionary[(byte)(bytes[0] >> 6)]);
            list.Add("Serial Number: Message code: " + (((bytes[0] & 0x3f) * 16) + ((bytes[1]) >> 4)));
            list.Add("Serial Number: Update number: " + (bytes[1] & 0xf));
            TableOutputController.Format(bytes.Take(2).ToArray(), list);

            TableOutputController.Format(bytes.Skip(2).Take(2).ToArray(), "Message Identifier: " + ((bytes[2] << 8) | bytes[3]));
            DCSEnum alphabet = GsmAlphabet.DecodeDCS(bytes[4]);
            TableOutputController.Format(bytes[4], "Data Coding Scheme");
            TableOutputController.Format(bytes[5], "Page Parameter");
            TableOutputController.Format(bytes.Skip(6).ToArray(), "Content of Message");
        }

        static void LocationStatusHandler(byte[] bytes)
        {
            Dictionary<byte, string> dictionary = new Dictionary<byte, string>
            {
                {0x00, "Normal service"},
                {0x01, "Limited service"},
                {0x02, "No service"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(dictionary, bytes[0], "reserved value"));
        }

        static void BrowserTerminationCauseHandler(byte[] bytes)
        {
            Dictionary<byte, string> dictionary = new Dictionary<byte, string>
            {
                {0x00, "Browser Termination Cause: User Termination"},
                {0x01, "Browser Termination Cause: Error Termination"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(dictionary, bytes[0], "reserved value"));
        }

        static void TrackingAreaIdentificationHandler(byte[] bytes)
        {
            if (bytes.Length != 5)
            {
                HandleLengthError(bytes);
                return;
            }
            DecodePLMN(bytes.Take(3).ToArray());
            TableOutputController.Format(bytes.Skip(3).ToArray(), "Tracking Area Code");
        }


        static void UpdateAttachTypeHandler(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                HandleLengthError(bytes);
            }
            Dictionary<byte, string> dictionary = new Dictionary<byte, string>
            {
                {0x00, "Normal Location Updating"},
                {0x01, "Periodic Updating"},
                {0x02, "IMSI Attach"},
                {0x03, "GPRS Attach"},
                {0x04, "Combined GPRS/IMSI Attach"},
                {0x05, "RA Updating"},
                {0x06, "Combined RA/LA Updating"},
                {0x07, "Combined RA/LA Updating with IMSI Attach"},
                {0x08, "Periodic Updating"},
                {0x09, "EPS Attach"},
                {0x0A, "Combined EPS/IMSI Attach"},
                {0x0B, "TA updating "},
                {0x0C, "Combined TA/LA updating"},
                {0x0D, "Combined TA/LA updating with IMSI attach"},
                {0x0E, "Periodic updating"},
            };
            TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(dictionary, bytes[0], "reserved value"));
        }

        static void RejectionCauseCodeHandler(byte[] bytes)
        {
            Dictionary<byte, string> dictionary = new Dictionary<byte, string>
            {
                {0x02, "IMSI unknown in HLR"},           
                {0x03, "Illegal MS"},
                {0x04, "IMSI unknown in VLR"},          
                {0x05, "IMEI not accepted"},             
                {0x06, "Illegal ME"},
                {0x07, "GPRS services not allowed"},     
                {0x08, "GPRS services and non-GPRS services not allowed"},  
                {0x09, "MS identity cannot be derived by the network"},     
                {0x0a, "Implicitly detached"},           
                {0x0b, "PLMN not allowed"},              
                {0x0c, "Location Area not allowed"},                
                {0x0d, "Roaming not allowed in this location area"},  
                {0x0e, "GPRS services not allowed in this PLMN"},
                {0x0f, "No Suitable Cells In Location Area"},       
                {0x10, "MSC temporarily not reachable"}, 
                {0x11, "Network failure"},     
                {0x14, "MAC failure"},
                {0x15, "Synch failure"},                 
                {0x16, "Congesttion"},
                {0x17, "GSM authentication unacceptable"},           
                {0x19, "Not authorized for this CSG"},            
                {0x20, "Service option not supported"},  
                {0x21, "Requested service option not subscribed"},   
                {0x22, "Service option temporarily out of order"},     
                {0x26, "Call cannot be identified"},     
                {0x28, "No PDP context activated"},      
                {0x5f, "Semantically incorrect msg"},    
                {0x60, "Invalid mandatory information"},        
                {0x61, "Message type non-existent or not implemented"},     
                {0x62, "Message type not compatible with the protocol state"},
                {0x63, "Information element non-existent or not implemented"},
                {0x64, "Conditional IE error"},  
                {0x65, "Message not compatible with the protocol state"},     
                {0x6f, "Protocol error, unspecified"},
            };
            if (bytes.Length != 1)
            {
                HandleLengthError(bytes);
                return;
            }
            if (bytes[0] >= 0x30 && bytes[0] <= 0x3f)
            {
                TableOutputController.Format(bytes[0], "Retry upon entry into a new cell");
            }
            else
            {
                TableOutputController.Format(bytes[0], GetValueOrDefaultFromDictionary(dictionary, bytes[0], "reserved value"));
            }
        }

        static void RoutingAreaInformationHandler(byte[] bytes)
        {
            if (bytes.Length != 6)
            {
                HandleLengthError(bytes);
                return;
            }
            DecodePLMN(bytes.Take(3).ToArray());
            TableOutputController.Format(bytes.Skip(3).Take(2).ToArray(), "Location Area Code");
            TableOutputController.Format(bytes[5], "Routing Area code");
        }

        //Handler函数只处理value部分，调用者负责保证传入的byte[]长度至少为1.
        static Dictionary<byte, TlvHandler> CatHandlerDictionary = new Dictionary<byte, TlvHandler> 
        {
            {0x01, CommandDetailsHandler},
            {0x02, DeviceIdentityHandler},
            {0x03, ResultHandler},
            {0x04, DurationHandler},
            {0x05, AlphaIdentifierHandler},
            {0x06, AddressHandler},
            {0x09, SSStringHandler},
            {0x0A, USSDStringHandler},
            {0x0B, SmsTpduHandler},
            {0x0C, CellBroadcastPageHandler},
            {0x0D, TextStringHandler},
            {0x0E, ToneHandler},
            {0x0f, ItemHandler},
            {0x10, ItemIdentifierHandler},
            {0x11, ResponseLengthHandler},
            {0x12, FileListHandler},
            {0x13, LocationInformationHandler},
            {0x14, IMEIHandler},
            //{0x16, NetworkMeasurementResultsHandler},
            {0x19, EventListHandler},
            {0x1B, LocationStatusHandler},
            {0x1E, IconIdentifierHandler},
            {0x24, TimerIdentifierHandler},
            {0x25, TimerValueHandler},
            {0x26, DateTimeAndTimezoneHandler},
            {0x2C, DTMFStringHandler},
            {0x2D, LanguageHandler},
            {0x2E, TimingAdvanceHandler},
            {0x31, URLHandler},
            {0x34, BrowserTerminationCauseHandler},
            {0x35, BearerDescriptionHandler},
            //{0x36, ChannelDataHandler},
            {0x37, ChannelDataLengthHandler},
            {0x38, ChannelStatusHandler},
            {0x39, BufferSizeHandler},
            {0x3C, UICCTerminalInterfaceTransportLevelHandler},
            {0x3E, OtherAddressHandler},
            {0x3F, AccessTechnologyHandler},
            {0x47, NetworkAccessNameHandler},
            {0x50, TextAttributeHandler},
            {0x62, IMEISVHandler},
            {0x65, NetworkSearchModeHandler},
            {0x72, PLMNwAcTListHandler},
            {0x73, RoutingAreaInformationHandler},
            {0x74, UpdateAttachTypeHandler},
            {0x75, RejectionCauseCodeHandler},
            {0x7d, TrackingAreaIdentificationHandler},
        };



        //31.111 9.3	COMPREHENSION-TLV tags in both directions，对ts102223进行了扩展
        static Dictionary<byte, string> CatDataObjDictionary = new Dictionary<byte, string>
        {
            {0x01, "Command details"},
            {0x02, "Device identity"},
            {0x03, "Result"},
            {0x04, "Duration"},
            {0x05, "Alpha identifier"},
            {0x06, "Address"},
            {0x07, "Capability configuration parameters"},
            {0x08, "Subaddress"},
            {0x09, "SS string"},
            {0x0A, "USSD string"},
            {0x0B, "SMS TPDU"},
            {0x0C, "Cell Broadcast page"},
            {0x0D, "Text string"},
            {0x0E, "Tone"},
            {0x0F, "Item"},
            {0x10, "Item identifier"},
            {0x11, "Response length"},
            {0x12, "File List"},
            {0x13, "Location Information"},
            {0x14, "IMEI"},
            {0x15, "Help request"},
            {0x16, "Network Measurement Results"},
            {0x17, "Default Text"},
            {0x18, "only Items Next Action Indicator"},
            {0x19, "Event list"},
            {0x1A, "Cause"},
            {0x1B, "Location status"},
            {0x1C, "Transaction identifier"},
            {0x1D, "BCCH channel list"},
            {0x1E, "Icon identifier"},
            {0x1F, "Item Icon identifier list"},
            {0x20, "Card reader status"},
            {0x21, "Card ATR"},
            {0x22, "C-APDU"},
            {0x23, "R-APDU"},
            {0x24, "Timer identifier"},
            {0x25, "Timer value"},
            {0x26, "Date-Time and Time zone"},
            {0x27, "Call control requested action"},
            {0x28, "AT Command"},
            {0x29, "AT Response"},
            {0x2A, "BC Repeat Indicator"},
            {0x2B, "Immediate response"},
            {0x2C, "DTMF string"},
            {0x2D, "Language"},
            {0x2E, "Timing Advance"},
            {0x2F, "AID"},
            {0x30, "Browser Identity"},
            {0x31, "URL"},
            {0x32, "Bearer"},
            {0x33, "Provisioning Reference File"},
            {0x34, "Browser Termination Cause"},
            {0x35, "Bearer description"},
            {0x36, "Channel data"},
            {0x37, "Channel data length"},
            {0x38, "Channel status"},
            {0x39, "Buffer size"},
            {0x3A, "Card reader identifier"},
            {0x3B, "File Update Information"},
            {0x3C, "UICC/terminal interface transport level"},
            //{0x3D, "Not used"},
            {0x3E, "Data destination address"},//Other address
            {0x3F, "Access Technology"},
            {0x40, "Display parameters"},
            {0x41, "Service Record"},
            {0x42, "Device Filter"},
            {0x43, "Service Search"},
            {0x44, "Attribute information"},
            {0x45, "Service Availability"},
            {0x46, "ESN"},
            {0x47, "Network Access Name"},
            {0x48, "CDMA-SMS-TPDU"},
            {0x49, "Remote Entity Address"},
            {0x4A, "I-WLAN Identifier"},
            {0x4B, "I-WLAN Access Status"},
            {0x50, "Text attribute"},
            {0x51, "Item text attribute list"},
            {0x52, "PDP context Activation parameters"},
            {0x55, "CSG cell selection status"},
            {0x56, "CSG ID"},
            {0x57, "HNB name"},
            {0x62, "IMEISV"},
            {0x63, "Battery state"},
            {0x64, "Browsing status"},
            {0x65, "Network Search Mode"},
            {0x66, "Frame Layout"},
            {0x67, "Frames Information"},
            {0x68, "Frame identifier"},
            {0x69, "UTRAN/E-UTRAN Measurement Qualifier"},
            {0x6A, "Multimedia Message Reference"},
            {0x6B, "Multimedia Message Identifier"},
            {0x6D, "MEID"},
            {0x6E, "Multimedia Message Content Identifier"},
            {0x6F, "Multimedia Message Notification"},
            {0x70, "Last Envelope"},
            {0x71, "Registry application data"},
            {0x72, "PLMNwAcT List"},
            {0x73, "Routing Area Information"},
            {0x74, "Update/Attach Type"},
            {0x75, "Rejection Cause Code"},
            {0x76, "Geographical Location Parameters"},
            {0x77, "GAD Shapes"},
            {0x78, "NMEA sentence"},
            {0x79, "PLMN List"},
            {0x7A, "Broadcast Network Information"},
            {0x7B, "ACTIVATE descriptor"},
            {0x7c, "EPS PDN connection Activation parameters "},
            {0x7d, "Tracking Area Identification"},
            {0x7e, "CSG ID"},
        };
        private static byte CommandInProcess = 0;
        static Dictionary<byte, string> CatTypeOfCommandDictionary = new Dictionary<byte, string>
        {
            {0x01, "REFRESH"},
            {0x02, "MORE TIME"},
            {0x03, "POLL INTERVAL"},
            {0x04, "POLLING OFF"},
            {0x05, "SET UP EVENT LIST"},
            {0x10, "SET UP CALL"},
            {0x11, "SEND SS"},
            {0x12, "SEND USSD"},
            {0x13, "SEND SHORT MESSAGE"},
            {0x14, "SEND DTMF"},
            {0x15, "LAUNCH BROWSER"},
            {0x20, "PLAY TONE"},
            {0x21, "DISPLAY TEXT"},
            {0x22, "GET INKEY"},
            {0x23, "GET INPUT"},
            {0x24, "SELECT ITEM"},
            {0x25, "SET UP MENU"},
            {0x26, "PROVIDE LOCAL INFORMATION"},
            {0x27, "TIMER MANAGEMENT"},
            {0x28, "SET UP IDLE MODE TEXT"},
            {0x30, "PERFORM CARD APDU"},
            {0x31, "POWER ON CARD"},
            {0x32, "POWER OFF CARD"},
            {0x33, "GET READER STATUS"},
            {0x34, "RUN AT COMMAND"},
            {0x35, "LANGUAGE NOTIFICATION"},
            {0x40, "OPEN CHANNEL"},
            {0x41, "CLOSE CHANNEL"},
            {0x42, "RECEIVE DATA"},
            {0x43, "SEND DATA"},
            {0x44, "GET CHANNEL STATUS"},
            {0x45, "SERVICE SEARCH"},
            {0x46, "GET SERVICE INFORMATION"},
            {0x47, "DECLARE SERVICE"},
            {0x50, "SET FRAMES"},
            {0x51, "GET FRAMES STATUS"},
            {0x62, "DISPLAY MULTIMEDIA MESSAGE"},
            {0x70, "ACTIVATE"},
            {0x71, "CONTACTLESS STATE CHANGED"},
            {0x72, "COMMAND CONTAINER"},
            {0x73, "ENCAPSULATED SESSION CONTROL"},
            {0x81, "End of the proactive UICC session"},
        };
        static Dictionary<byte, string> ResultDictionary = new Dictionary<byte, string>
        {
            {0x00, "Command performed successfully"},
            {0x01, "Command performed with partial comprehension"},
            {0x02, "Command performed, with missing information"},
            {0x03, "REFRESH performed with additional Efs read"},
            {0x04, "Command performed successfully, but requested icon could not be displayed"},
            {0x05, "Command performed, but modified by call control by NAA"},
            {0x06, "Command performed successfully, limited service"},
            {0x07, "Command performed with modification"},
            {0x08, "REFRESH performed but indicated NAA was not active"},
            {0x09, "Command performed successfully, tone not played"},
            {0x10, "Proactive UICC session terminated by the user"},
            {0x11, "Backward move in the proactive UICC session requested by the user"},
            {0x12, "No response from user"},
            {0x13, "Help information required by the user"},
            {0x14, "USSD or SS transaction terminated by the user"},
            {0x20, "terminal currently unable to process command"},
            {0x21, "Network currently unable to process command"},
            {0x22, "User did not accept the proactive command"},
            {0x23, "User cleared down call before connection or network release"},
            {0x24, "Action in contradiction with the current timer state"},
            {0x25, "Interaction with call control by NAA, temporary problem"},
            {0x26, "Launch browser generic error code"},
            {0x27, "MMS temporary problem"},
            {0x30, "Command beyond terminal's capabilities"},
            {0x31, "Command type not understood by terminal"},
            {0x32, "Command data not understood by terminal"},
            {0x33, "Command number not known by terminal"},
            {0x34, "SS Return Error"},
            {0x35, "SMS RP-ERROR"},
            {0x36, "Error, required values are missing"},
            {0x37, "USSD Return Error"},
            {0x38, "MultipleCard commands error"},
            {0x39, "Interaction with call control by USIM or MO short message control by USIM, permanent problem"},
            {0x3A, "Bearer Independent Protocol error"},
            {0x3B, "Access Technology unable to process command"},
            {0x3C, "Frames error"},
            {0x3D, "MMS Error"},
        };
        static Dictionary<byte, string> SupsErrorCodeDictionary = new Dictionary<byte, string>
        {
            {0x01, "Unknown subscriber"},
            {0x09, "Illegal subscriber"},
            {0x0A, "Bearer service not provisioned"},
            {0x0B, "Telephony service not provisioned"},
            {0x0D, "Call barred"},
            {0x0C, "Illegal equipment"},
            {0x10, "Illegal operation"},
            {0x11, "Status error"},
            {0x12, "SUPS not available"},
            {0x13, "Subscription violation"},
            {0x14, "SUPS incompatibility"},
            {0x15, "Facility not supported"},
            {0x1B, "Absent subscriber"},
            {0x1D, "Short term denial"},
            {0x1E, "Long term denial"},
            {0x22, "System failure"},
            {0x23, "Data missing"},
            {0x24, "Unexpected data value"},
            {0x25, "Password registration failure"},
            {0x26, "Negative password check"},
            {0x2B, "Number of password attempts violation"},
            {0x36, "Position method failure"},
            {0x47, "Unknown alphabet"},
            {0x48, "USSD busy"},
            {0x79, "Rejected by user"},
            {0x7A, "Rejected by network"},
            {0x7B, "Deflection to served subscriber"},
            {0x7C, "Special service code"},
            {0x7D, "Invalid Deflected-To number"},
            {0x7E, "Maximum number of Multi-Party participants exceeded"},
            {0x7F, "Resources not available"},
        };
    }
}
