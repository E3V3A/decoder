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
        private uint RawIndex = 0;
        private string TimeStamp = null;
        private int Code = 0;
        private string Name = null;
        private List<string> Content = new List<string>();
        private StreamWriter ApduFileWriter = null;
        private StreamWriter MsgFileWriter = null;
        private uint ApduCounter = 0;
        
        private string IMEI = null;
        private string IMSI = null;
        private string BuildID = null;
        //private string Slot = "0";

        public Item()
        {
        }

        public void Close()
        {
            if (ApduFileWriter != null) ApduFileWriter.Close();
            if (MsgFileWriter != null) MsgFileWriter.Close();
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
        public void Init(uint index, string time, string code, string name)
        {
            Clear();
            RawIndex = index;
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
                    return String.Format("SFI: 0x{0:X2}", (sfi >> 3));
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
                    return "SFI: " + match.Groups[1].Value;
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
                    return match.Groups[1].Value;
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
                    return match.Groups[1].Value;
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
            return part1 + " " + part2;
        }

        private void WriteFile(StreamWriter sw, string line)
        {
            if (Code == 0x1FEB)
            {
                sw.WriteLine(String.Format("{0,-7} {1} 0x{2:X4} {3}", RawIndex, TimeStamp, Code, line));
            }
            else
            {
                sw.WriteLine(String.Format("{0,-7} {1} 0x{2:X4} {3} {4}", RawIndex, TimeStamp, Code, Name, line));
            }
        }

        private void WriteDebugFile(string line)
        {
            if (MsgFileWriter == null)
            {
                MsgFileWriter = new StreamWriter(Utils.MsgFileName);
            }
            WriteFile(MsgFileWriter, line);
        }


        private void WriteApduFile(string line)
        {
            if (ApduFileWriter == null)
            {
                ApduFileWriter = new StreamWriter(Utils.ApduFileName);
            }
            WriteFile(ApduFileWriter, line);
            WriteDebugFile(line);
        }

        //传入的数据以"APDU Parsing"开头
        private string GetAPDUSummary(List<string> text)
        {
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

            if (handler != null)
            {
                string extra = handler(text, index);
                if (extra != null)
                {
                    return String.Format("Summary: {0}, {1}", apduType, extra);
                }
            }
            return String.Format("Summary: {0}", apduType);
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
                    WriteApduFile(String.Format("{0}: {1}", match.Groups[1].Value, match.Groups[2].Value));
                    continue;
                }

                if (Content[index].Contains("APDU Parsing"))
                {
                    WriteApduFile(GetAPDUSummary(Content.Skip(index).ToList()));
                    //break;//有些日志在APDU Parsing后仍然存在APDU行
                }
            }
        }

        private void Handle14CE()
        {
            string direction = null;
            StringBuilder apdu = new StringBuilder();
            Match match = null;
            int index = 0;
            bool found = false;
            string slot = null;

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
            WriteApduFile(String.Format("SLOT{0} {1}: {2}", slot, direction, apdu.ToString()));

            if (found)
            {
                WriteApduFile(String.Format("SLOT{0} {1}", slot, GetAPDUSummary(Content.Skip(index).ToList())));
            }
        }

        private void HandleOTA()
        {
            WriteDebugFile("");
        }

        private void HandleQMI2()
        {
            if (Content.Count < 5)
            {
                return;
            }
            string client = null;
            string ctlflags = null;
            string txid = null;
            string msgtype = null;
            Match match = null;
            int index = 0;
            string extra = null;

            index += 4;
            match = Regex.Match(Content[index], @"(.*) {");
            if (match.Success)
            {
                //MsgType在括号内的情况
                if (Content.Count < 9)
                {
                    return;
                }

                index++;
                match = Regex.Match(Content[index], @"   (.*)");
                client = match.Groups[1].Value;

                index++;
                match = Regex.Match(Content[index], @"   (.*)");
                ctlflags = match.Groups[1].Value;

                index++;
                match = Regex.Match(Content[index], @"   (.*)");
                txid = match.Groups[1].Value;

                index++;
                match = Regex.Match(Content[index], @"   (.*)");
                msgtype = match.Groups[1].Value;

                if (msgtype == "MsgType = QMI_UIM_CHANGE_PROVISIONING_SESSION")
                {
                    if (ctlflags == "SduCtlFlags = REQ")
                    {
                        string session = null;
                        string action = null;
                        string slot = null;
                        if (Content.Count >= 25)
                        {
                            index += 8;
                            match = Regex.Match(Content[index], @"            (.*)");
                            session = match.Groups[1].Value;
                            index++;
                            match = Regex.Match(Content[index], @"            (.*)");
                            action = match.Groups[1].Value;
                            index += 7;
                            match = Regex.Match(Content[index], @"            (.*)");
                            slot = match.Groups[1].Value;
                            extra = session + " " + action + " " + slot;
                        }
                    }
                    else if (ctlflags == "SduCtlFlags = RSP")
                    {
                        if (Content.Count >= 17)
                        {
                            index += 8;
                            match = Regex.Match(Content[index], @"            (.*)");
                            extra = match.Groups[1].Value;
                        }
                    }
                }

            }
            else
            {
                match = Regex.Match(Content[index], @"(ClientId = .*)");
                if (match.Success)
                {
                    if (Content.Count < 10)
                    {
                        return;
                    }
                    client = Content[index];
                    index++;
                    ctlflags = Content[index];
                    index++;
                    txid = Content[index];
                    index++;
                    msgtype = Content[index];
                }
            }

            /*if (msgtype == null || (!msgtype.StartsWith("MsgType = QMI_UIM") && !msgtype.StartsWith("MsgType = QMI_CAT") && !msgtype.StartsWith("MsgType = QMI_PBM")))
            {
                WriteQMIFile(String.Format("{0,-14}{1,-10} {2,-18} {3}", client, txid, ctlflags, msgtype));
                return;
            }*/

            WriteDebugFile(String.Format("{0,-14}{1,-10} {2,-18} {3} {4}", client, txid, ctlflags, msgtype, extra));
        }


        private void HandleQMI()
        {
            if (Content.Count < 12)
            {
                return;
            }

            string type = null;
            string counter = null;
            string service = null;
            string command = null;
            Match match = null;
            int index = 0;

            index++;
            type = Content[index];

            index++;
            counter = Content[index];

            index += 7;
            match = Regex.Match(Content[index], @"(.*) {");
            if (!match.Success)
            {
                return;
            }
            service = match.Groups[1].Value;

            index += 2;// 当前index预期指向command
            match = Regex.Match(Content[index], @"      (.*) {");
            if (!match.Success)
            {
                return;
            }
            command = match.Groups[1].Value;

            string extra = null;
            if (command == "uim_change_provisioning_session")
            {
                if (type.Contains("Request"))
                {
                    if (Content.Count > (index + 6))
                    {
                        string slot = null;
                        string session = null;
                        string action = null;

                        index += 5;
                        session = Content[index].Substring(15);

                        index++;
                        action = Content[index].Substring(15);

                        if (Content.Count > (index + 7))
                        {
                            index += 7;
                            slot = Content[index].Substring(15);
                        }

                        extra = session + " " + action + " " + slot;
                    }
                }
                else if (type.Contains("Response"))
                {
                    if (Content.Count >= 18)
                    {
                        index += 5;
                        extra = Content[index].Substring(15);
                    }
                }
            }
            else if (command == "uim_power_down")
            {
                if (Content.Count > (index + 5))
                {
                    index += 5;
                    extra = Content[index].Substring(15);
                }
            }
            else if (command == "pbm_all_pb_init_done")
            {
                if (Content.Count > (index + 8))
                {
                    string session = null;
                    string mask = null;

                    index += 7;
                    if (Content[index].Length > 18)
                    {
                        session = Content[index].Substring(18);

                        index++;
                        mask = Content[index].Substring(18);

                        extra = session + " " + mask;
                    }
                }
            }
            else if ((command == "uim_write_record" || command == "uim_read_record" || command == "uim_read_transparent") && type.Contains("Request"))
            {
                if (Content.Count > (index + 13))
                {
                    string session = null;

                    index += 5;
                    match = Regex.Match(Content[index], @"               (.*)");
                    session = match.Groups[1].Value;

                    index += 8;
                    while (index < Content.Count)
                    {
                        match = Regex.Match(Content[index], @"               file_id = (.*)");
                        index++;
                        if (match.Success)
                        {
                            extra = String.Format("{0} file_id = 0x{1:X}", session, Convert.ToInt32(match.Groups[1].Value));
                            break;
                        }
                    }
                }
            }
            WriteDebugFile(String.Format("{0,-13} {1,-16} {2,-20} {3} {4}", counter, service, type, command, extra));
        }

        private void HandleDebugMsg()
        {
            if (Content.Count == 0)
            {
                return;
            }
            Match match = Regex.Match(Content[0], @"\t(\S*)\t(\d*)\t(\w)\t(.*)");
            if (!match.Success)
            {
                return;
            }
            WriteDebugFile(String.Format("{0,-20} {1,5} {2} {3}", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value));
        }

        private void HandleDiagRsp()
        {
            int index = 0;
            Match match = null;
            if (Name.Contains("Extended Build ID"))
            {
                string buildId = null;
                index += 3;
                if (index >= Content.Count)
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
                        WriteApduFile(String.Format("{0}", buildId));
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
                    WriteApduFile(String.Format("IMEI: {0} IMSI: {1}", imei, imsi));
                    IMEI = imei;
                    IMSI = imsi;
                }
            }

        }

        public uint GetApduCount()
        {
            return ApduCounter;
        }


        /*0x138e, 0x138f, 0x1390, 0x1391, 0x1544, //QMI
                    0xb0c0, 0xb0e2, 0xb0e3, 0xb0ec, 0xb0ed, //OTA LTE
                    0x713a, 0x7b3a, 0xd0e3, 0x412f,         //OTA  UMTS, TDS, W
                    0x1004, 0x1005, 0x1006, 0x1007, 0x1008, //OTA 1X
        */
        public void Process()
        {
            if (IsValidItem())
            {
                switch (Code)
                {
                    case 0x14ce:
                        ApduCounter++;
                        Handle14CE();
                        break;
                    case 0x1098:
                        ApduCounter++;
                        Handle1098();
                        break;
                    case 0x1ff0:
                        HandleDiagRsp();
                        break;
                    case 0x1feb:
                        HandleDebugMsg();
                        break;
                    case 0x1544:
                        HandleQMI();
                        break;
                    case 0x138e:
                    case 0x138f:
                    case 0x1390:
                    case 0x1391:
                    case 0x1392:
                    case 0x1393:
                    case 0x1394:
                    case 0x1395:
                    case 0x1396:
                    case 0x1397:
                    case 0x1398:
                    case 0x1399:
                    case 0x139a:
                    case 0x139b:
                    case 0x139c:
                    case 0x139d:
                    case 0x139e:
                    case 0x139f:
                    case 0x13a0:
                    case 0x13a1:
                    case 0x13a2:
                    case 0x13a3:
                    case 0x13a4:
                    case 0x13a5:
                    case 0x13a6:
                    case 0x13a7:
                    case 0x13a8:
                    case 0x13a9:
                    case 0x13aa:
                    case 0x13ab:
                    case 0x13ac:
                    case 0x13ad:
                        HandleQMI2();
                        break;
                ////0xb0c0, 0xb0e2, 0xb0e3, 0xb0ec, 0xb0ed, //OTA LTE
                ////0x713a, 0x7b3a, 0xd0e3, 0x412f, 0x5b2f, //OTA  UMTS, TDS, W, GSM
                ////0x1004, 0x1005, 0x1006, 0x1007, 0x1008, //OTA 1X
                ////0x156e, 0x1830, 0x1831, 0x1832, //IMS
                    case 0xb0c0:
                    case 0xb0e2:
                    case 0xb0e3:
                    case 0xb0ec:
                    case 0xb0ed:
                    case 0x713a:
                    case 0x7b3a:
                    case 0xd0e3:
                    case 0x412f:
                    case 0x5b2f:
                    case 0x1004:
                    case 0x1005:
                    case 0x1006:
                    case 0x1007:
                    case 0x1008:
                    case 0x156e:
                    case 0x1830:
                    case 0x1831:
                    case 0x1832:
                        HandleOTA();
                        break;
                }
            }
        }
    }
}
