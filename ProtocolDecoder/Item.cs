using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace ProtocolDecoder
{
    class Item
    {
        private string TimeStamp = null;
        private int Code = 0;
        private string Name = null;
        private List<string> Content = new List<string>();
        private StreamWriter ApduFileWriter = null;
        private StreamWriter MsgFileWriter = null;
        private StreamWriter CardFileWriter = null;
        private StreamWriter StkFileWriter = null;
        private bool NeedSummary = false;
        private string IMEI = null;
        private string IMSI = null;
        private string BuildID = null;

        public Item( bool hasSummary)
        {
            NeedSummary = hasSummary;
        }

        public void Close()
        {
            if (ApduFileWriter != null) ApduFileWriter.Close();
            if ( MsgFileWriter!=null) MsgFileWriter.Close();
            if (CardFileWriter != null) CardFileWriter.Close();
            if (StkFileWriter != null) StkFileWriter.Close();
        }

        public bool IsValidItem()
        {
            return (Content.Count != 0 && TimeStamp != null);
        }
        public void Clear()
        {
            TimeStamp = null;
            Content.Clear();
        }
        public void Add(string line)
        {
            Content.Add(line);
        }
        public void Init(string time, string code, string name)
        {
            Clear();
            TimeStamp = time;
            Code = Convert.ToInt32(code, 16);
            Name = name;
        }

        delegate string ParsedApduHandler(List<string> text, int index);
        Dictionary<string, ParsedApduHandler> ParsedApduHandlerDictionary = new Dictionary<string, ParsedApduHandler> 
        {
            {"SELECT", SelectHandler},
            {"READ BINARY", BinaryHandler},
            {"UPDATE BINARY", BinaryHandler},
            {"READ RECORD", RecordHandler},
            {"UPDATE RECORD", RecordHandler},
            {"FETCH", PoractiveHandler},
            {"TERMINAL RESPONSE", PoractiveHandler},
            {"ENVELOPE", EnvelopHandler}
        };

        static string RecordHandler(List<string> text, int index)
        {
            Match match = null;
            index += 3;
            while (text.Count > index)
            {
                match = Regex.Match(text[index], @"SFI: (.*)");
                index++;
                if (match.Success)
                {
                    UInt16 sfi = Convert.ToUInt16(match.Groups[1].Value, 16);
                    return Format(String.Format("SFI: 0x{0:X2}", (sfi >> 3)));
                }
            }
            return null;
        }

        static string BinaryHandler(List<string> text, int index)
        {
            Match match = null;
            index += 3;
            while (text.Count > index)
            {
                match = Regex.Match(text[index], @"SFI: (.*)");
                index++;
                if (match.Success)
                {
                    return Format("SFI: " + match.Groups[1].Value);
                }
            }
            return null;
        }

        static string EnvelopHandler(List<string> text, int index)
        {
            Match match = null;
            index += 3;
            while (text.Count > index)
            {
                match = Regex.Match(text[index], @"(.*) -- Command Data");
                index++;
                if (match.Success)
                {
                    return Format(match.Groups[1].Value);
                }
            }
            return null;
        }

        static string PoractiveHandler(List<string> text, int index)
        {
            Match match = null;
            index += 5;
            while (text.Count > index)
            {
                match = Regex.Match(text[index], @"    Command Type               : (.*)");
                index++;
                if (match.Success)
                {
                    return Format(match.Groups[1].Value);
                }
            }
            return null;
        }

        static string SelectHandler(List<string> text, int index)
        {
            Match match = null;
            string part1 = null;
            string part2 = null;
            index += 3;
            while (text.Count > index)
            {
                match = Regex.Match(text[index], @"File ID: (.*)");
                index++;
                if (match.Success)
                {
                    part1 = "File: " + match.Groups[1].Value;
                    break;
                }
            }
            if (part1 == null)
            {
                return null;
            }
            index += 8;
            while (text.Count > index)
            {
                match = Regex.Match(text[index], @"  Short File Identifier        : (.*)");
                index++;
                if (match.Success)
                {
                    part2 = "SFI: " + match.Groups[1].Value;
                    break;
                }
            }
            return Format(part1, part2);
        }

        private static string Format(string part1, string part2 = null)
        {
            if (part2 == null)
            {
                return ", " + part1;
            }
            else
            {
                return ", " + part1 + ", " + part2;
            }
        }
        
        private void WriteApduFile(string line)
        {
            if(ApduFileWriter == null)
            {
                ApduFileWriter = new StreamWriter(Utils.BaseFileName + "_uim_apdu.txt");
            }
            ApduFileWriter.WriteLine(line);
        }

        //传入的数据以"APDU Parsing"开头
        private void ProcessParsedAPDU(List<string> text)
        {
            if (!NeedSummary)
            {
                return;
            }
            string apduType = null;
            int index = 2; ;

            if (text.Count > index)
            {
                apduType = text[index];
                index++;
                if (apduType.Contains("incomplete APDU") && (text.Count > index))
                {
                    apduType = text[index];
                    index++;
                }
            }
            ParsedApduHandler handler = null;
            ParsedApduHandlerDictionary.TryGetValue(apduType, out handler);
            StringBuilder output = new StringBuilder("                     Summary: " + apduType);
            if (handler != null)
            {
                output.Append(handler(text, index));
            }
            WriteApduFile(output.ToString());
        }

        private void Handle1098()
        {
            Match match = null;
            int index = 0;

            for (index = 0; index < Content.Count; index++)
            {
                match = Regex.Match(Content[index], @"\t{4}([TR]X) {10}(.*)");
                if (match.Success)
                {
                    WriteApduFile(String.Format("{0}, SLOT{1}, {2}: {3}", TimeStamp, 0, match.Groups[1].Value, match.Groups[2].Value));
                    continue;
                }

                if (Content[index].Contains("APDU Parsing"))
                {
                    ProcessParsedAPDU(Content.Skip(index).ToList());
                    break;
                }
            }
        }

        private void Handle14CE()
        {
            string slot = "0";
            string direction = null;
            StringBuilder apdu = new StringBuilder();
            Match match = null;
            int index = 0;
            bool found = false;

            for (index = 0; index < Content.Count; index++)
            {
                match = Regex.Match(Content[index], @"\|   ([TR]X) Data\|   (\d)\|  (..)\|");
                if (match.Success)
                {
                    direction = match.Groups[1].Value;
                    slot = match.Groups[2].Value;
                    apdu.Append(match.Groups[3].Value + " ");
                    continue;
                }

                if (Content[index].Contains("APDU Parsing"))
                {
                    found = true;
                    break;
                }
            }
            WriteApduFile(String.Format("{0}, SLOT{1}, {2}: {3}", TimeStamp, slot, direction, apdu.ToString()));

            if (found)
            {
                ProcessParsedAPDU(Content.Skip(index).ToList());
            }
        }

        private void HandleDebugMsg()
        {
            if(Content.Count == 0)
            {
                return;
            }
            Match match = Regex.Match(Content[0], @"\t(\S*)\t(\d*)\t(\w)\t(.*)");
            if(!match.Success)
            {
                return;
            }
            string output = String.Format("{0} {1,20} {2,5} {3} {4}", TimeStamp, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value);

            if (MsgFileWriter == null) MsgFileWriter = new StreamWriter(Utils.BaseFileName + "_msg.txt");
            MsgFileWriter.WriteLine(output);
            
            string cardFilter = @"gstk_init\s|Debounce logic ended successfully|Delay the powerup by|uim power up @|uim power down @|atr byte\[0|No. of Apps present|DF Present|Received MMGSDI_NOTIFY_LINK_EST_REQ|NV_UIM_SELECT_DEFAULT_USIM_APP_I Read|Handling cmd:0x26|Handling cmd:0x29|Handling cmd:0x30|Handling cmd:0x38|mmgsdi_util_select_first_app|Timed out on the command response|MMGSDI_FEATURE_MULTISIM_AUTO_PROVISIONING|Auto provisioning from MMGSDI EFS for App_type =|MMGSDI_CARD_INSERTED_EVT, slot:|MMGSDI_SESSION_CHANGED_EVT, app:|MMGSDI_PIN1_EVT, status:|MMGSDI_PERSO_EVT,|MMGSDI_SUBSCRIPTION_READY_EVT, app:|mmgsdi_session_manage_illegal_subscription|REMOVED_EVT, condition:";
            string stkFilter = @"gstk_process_envelope_cmd:|In gstk_process_proactive_command|SENDING REFRESH REQ TO MMGSDI|END Proactive session|Generating CAT EVT REPORT IND for Proactive Cmd|IN GSTK_OPEN_CH_REQ|IN GSTK_PROVIDE_LOCAL_INFO_REQ|IN GSTK_SEND_DATA_REQ:|IN GSTK_MORE_TIME_REQ|IN GSTK_SETUP_MENU_REQ|IN GSTK_SETUP_IDLE_TXT_REQ|IN GSTK_CLOSE_CH_REQ|IN GSTK_SETUP_EVT_LIST_REQ|IN GSTK_RECEIVE_DATA_REQ:|IN GSTK_SEND_SS_REQ|IN GSTK_TIMER_MANAGEMENT_REQ|IN GSTK_polling_REQ|IN GSTK_SEND_ussd_REQ|Received Term rsp|gstk_send_raw_terminal_response|UIM envelope rsp queued to front of GSTK queue";
            Match filter = null;
            filter = Regex.Match(match.Groups[4].Value, cardFilter);
            if(filter.Success)
            {
                if(CardFileWriter == null) CardFileWriter = new StreamWriter(Utils.BaseFileName + "_msg_card_init.txt");
                CardFileWriter.WriteLine(output);
            }
            filter = Regex.Match(match.Groups[4].Value, stkFilter);
            if (filter.Success)
            {
                if (StkFileWriter == null) StkFileWriter = new StreamWriter(Utils.BaseFileName + "_msg_stk.txt");
                StkFileWriter.WriteLine(output);
            }
        }

        private void HandleDiagRsp()
        {
            int index = 0;
            Match match = null;
            if (Name.Contains("Extended Build ID"))
            {
                string buildId = null;
                index += 3;
                if(index >= Content.Count)
                {
                    return;
                }
                match = Regex.Match(Content[index], @"Build ID and Model = (.*)");
                index++;
                if (match.Success && match.Groups[1].Value != "")
                {
                    buildId = match.Groups[1].Value;
                    if (buildId != BuildID)
                    {
                        BuildID = buildId;
                        WriteApduFile(String.Format("{0},        Build ID: {1}", TimeStamp, buildId));
                    }
                }
            }
            else if (Name.Contains("GSM Status Response") || Name.Contains("WCDMA Additional Status Response"))
            {
                string imei = null;
                string imsi = null;
                index += 3;
                if (index < Content.Count)
                {
                    match = Regex.Match(Content[index], @"IMEI: (.*)");
                    index++;
                    if (match.Success)
                    {
                        imei = match.Groups[1].Value;
                    }
                }
                if (index < Content.Count)
                {
                    if (Content[index].Contains("Invalid"))
                    {
                        return;
                    }
                    match = Regex.Match(Content[index], @"IMSI: (.*)");
                    index++;
                    if (match.Success)
                    {
                        imsi = match.Groups[1].Value;
                    }
                }
                if (IMEI != imei || IMSI != imsi)
                {
                    WriteApduFile(String.Format("{0},        IMEI: {1}, IMSI: {2}", TimeStamp, imei, imsi));
                    IMEI = imei;
                    IMSI = imsi;
                }
            }

        }

        public void Process()
        {
            if (IsValidItem())
            {
                switch (Code)
                {
                    case 0x14ce:
                        Handle14CE();
                        break;
                    case 0x1098:
                        Handle1098();
                        break;
                    case 0x1ff0:
                        HandleDiagRsp();
                        break;
                    case 0x1feb:
                        HandleDebugMsg();
                        break;
                }
            }
        }
    }
}
