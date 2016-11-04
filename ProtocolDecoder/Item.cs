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
        private StreamWriter Writer = null;
        private bool NeedSummary = false;
        private string IMEI = null;
        private string IMSI = null;
        private string BuildID = null;

        public Item(StreamWriter writer, bool hasSummary)
        {
            Writer = writer;
            NeedSummary = hasSummary;
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
            Writer.WriteLine(output.ToString());
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
                    Writer.WriteLine(String.Format("{0}, SLOT{1}, {2}: {3}", TimeStamp, 0, match.Groups[1].Value, match.Groups[2].Value));
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

            Writer.WriteLine(String.Format("{0}, SLOT{1}, {2}: {3}", TimeStamp, slot, direction, apdu.ToString()));

            if (found)
            {
                ProcessParsedAPDU(Content.Skip(index).ToList());
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
                match = Regex.Match(Content[index], @"Build ID and Model = (.*)");
                index++;
                if (match.Success && match.Groups[1].Value != "")
                {
                    buildId = match.Groups[1].Value;
                    if (buildId != BuildID)
                    {
                        BuildID = buildId;
                        Writer.WriteLine(String.Format("{0},        Build ID: {1}", TimeStamp, buildId));
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
                    Writer.WriteLine(String.Format("{0},        IMEI: {1}, IMSI: {2}", TimeStamp, imei, imsi));
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
                }
            }
        }
    }
}
