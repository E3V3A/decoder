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
        private StreamWriter QMIFileWriter = null;
        private StreamWriter MsgFileWriter = null;
        private StreamWriter OTAFileWriter = null;
        private uint ApduCounter = 0;
        //private StreamWriter CardFileWriter = null;
        //private StreamWriter StkFileWriter = null;
        private bool NeedSummary = false;
        private bool NeedOta = false;
        private bool NeedMsg = false;
        private string IMEI = null;
        private string IMSI = null;
        private string BuildID = null;
        //private string Slot = "0";

        public Item(bool hasSummary, bool needOta, bool needMsg)
        {
            NeedSummary = hasSummary;
            NeedOta = needOta;
            NeedMsg = needMsg;
        }

        public void CloseFile()
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

        private void WriteFileNoIndex(StreamWriter sw, string line)
        {
            StringBuilder sb = new StringBuilder(String.Format("{0,-7} {1} 0x{2:X4} ", RawIndex, TimeStamp, Code));

            if (Name != null)
            {
                sb.Append(Name + " ");
            }

            sw.WriteLine(sb.Append(line));
        }


        private void WriteDebugFile(string line)
        {
            if (MsgFileWriter == null)
            {
                MsgFileWriter = new StreamWriter(Utils.MsgFileName);
            }
            MsgFileWriter.WriteLine(TimeStamp+" "+line);
        }

        private void WriteFileWithIndex(StreamWriter sw, string line)
        {
            StringBuilder sb = new StringBuilder(String.Format("{0,-7} {1} 0x{2:X4} ", RawIndex, TimeStamp, Code));

            if (Name != null)
            {
                sb.Append(Name + " ");
            }

            sw.WriteLine(sb.Append(line));
        }

        private void WriteApduFile(string line)
        {
            if (ApduFileWriter == null)
            {
                ApduFileWriter = new StreamWriter(Utils.ApduFileName);
            }
            WriteFileWithIndex(ApduFileWriter, line);
        }

        private void WriteOTAFile(string line)
        {
            if (OTAFileWriter == null)
            {
                OTAFileWriter = new StreamWriter(Utils.OTAFileName);
            }
            WriteFileWithIndex(OTAFileWriter, line);
        }

        private void WriteQMIFile(string line)
        {
            if (QMIFileWriter == null)
            {
                QMIFileWriter = new StreamWriter(Utils.QMIFileName);
            }
            WriteFileWithIndex(QMIFileWriter, line);
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
            StringBuilder output = new StringBuilder();
            if (handler != null)
            {
                return String.Format("Summary: {0}{1}", apduType, handler(text, index));
            }
            else
            {
                return String.Format("Summary: {0}", apduType);
            }
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

                if (Content[index].Contains("APDU Parsing") && NeedSummary)
                {
                    WriteApduFile(GetAPDUSummary(Content.Skip(index).ToList()));
                    break;
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

            if (found && NeedSummary)
            {
                WriteApduFile(String.Format("SLOT{0} {1}", slot, GetAPDUSummary(Content.Skip(index).ToList())));
            }
        }

        private void HandleOTA()
        {
            WriteOTAFile("");
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

            WriteQMIFile(String.Format("{0,-14}{1,-10} {2,-18} {3} {4}", client, txid, ctlflags, msgtype, extra));
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

            index += 2;
            match = Regex.Match(Content[index], @"      (.*) {");
            if (!match.Success)
            {
                return;
            }
            command = match.Groups[1].Value;

            /*if (command == null || (!command.StartsWith("uim") && !command.StartsWith("cat") && !command.StartsWith("pbm")))
            {
                WriteQMIFile(String.Format("{0,-13} {1,-16} {2,-20} {3}", counter, service, type, command));
                return;
            }*/

            string extra = null;
            if (command == "uim_change_provisioning_session")
            {
                if (type == "MsgType = Request")
                {
                    if (Content.Count >= 25)
                    {


                        string slot = null;
                        string session = null;
                        string action = null;

                        index += 5;
                        match = Regex.Match(Content[index], @"               (.*)");
                        session = match.Groups[1].Value;

                        index++;
                        match = Regex.Match(Content[index], @"               (.*)");
                        action = match.Groups[1].Value;

                        index += 7;
                        match = Regex.Match(Content[index], @"               (.*)");
                        slot = match.Groups[1].Value;

                        extra = session + " " + action + " " + slot;
                    }
                }
                else if (type == "MsgType = Response")
                {
                    if (Content.Count >= 18)
                    {
                        index += 5;
                        match = Regex.Match(Content[index], @"               (.*)");
                        extra = match.Groups[1].Value;
                    }
                }
            }
            else if (command == "pbm_all_pb_init_done")
            {
                if (Content.Count >= 20)
                {
                    string session = null;
                    string mask = null;

                    index += 7;
                    match = Regex.Match(Content[index], @"                  (.*)");
                    session = match.Groups[1].Value;

                    index++;
                    match = Regex.Match(Content[index], @"                  (.*)");
                    mask = match.Groups[1].Value;
                    extra = session + " " + mask;
                }
            }
            else if ((command == "uim_write_record" || command == "uim_read_record" || command == "uim_read_transparent") && type == "MsgType = Request")
            {
                if (Content.Count >= 25)
                {
                    string session = null;
                    int file = 0;

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
                            file = Convert.ToInt32(match.Groups[1].Value);
                            extra = String.Format("{0} file_id = 0x{1:X}", session, file);
                            break;
                        }
                    }
                }
            }
            WriteQMIFile(String.Format("{0,-13} {1,-16} {2,-20} {3} {4}", counter, service, type, command, extra));
        }

        private void HandleDebugMsg()
        {
            Name = null;
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
                    case 0xb0c0:
                    case 0xb0e2:
                    case 0xb0e3:
                    case 0xb0ec:
                    case 0xb0ed:
                    case 0x713a:
                    case 0x7b3a:
                    case 0xd0e3:
                    case 0x412f:
                    case 0x1004:
                    case 0x1005:
                    case 0x1006:
                    case 0x1007:
                    case 0x1008:
                    case 0x5b2f:
                        HandleOTA();
                        break;
                }
            }
        }
    }
}
