using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;

namespace ProtocolDecoder
{
    class IsfDecoder
    {
        private static long GetApduFromText(string source, string target, bool hasSummary)
        {
            string line = null;
            Match match = null;
            long totalCount = 0;
            StreamReader reader = new StreamReader(source);
            StreamWriter writer = new StreamWriter(target);

            Item item = new Item(writer, hasSummary);
            while ((line = reader.ReadLine()) != null)
            {
                match = Regex.Match(line, @"^\d{4} .{6}  (..:..:..\....)  .{4}  0x(....)  (.*)");
                if (match.Success)
                {
                    item.Process();
                    totalCount++;
                    item.Init(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                }
                else
                {
                    item.Add(line);
                }
            }
            item.Process();
            totalCount++;

            reader.Close();
            writer.Close();
            return totalCount;
        }

        public static void ExtractApdu(string sourceIsf, BackgroundWorker bw, bool usePCTime, bool needSummary, bool needOTA)
        {
            string basePathName = Utils.GetFullBaseFileName(sourceIsf);
            //FileInfo fileInfo = new FileInfo(isfFile);
            long totalCount = 0;

            if (!Utils.IsValidIsf(sourceIsf))
            {
                bw.ReportProgress(0, "choose valid qxdm log");
                return;
            }

            bw.ReportProgress(1, "opening qxdm");
            if (!QXDMProcessor.Start(sourceIsf))
            {
                bw.ReportProgress(0, "can't open qxdm");
                return;
            }

            bw.ReportProgress(1, "extracting apdu isf log");
            string apduIsf = basePathName + "_apdu.isf";

            LogMask mask = new LogMask(apduIsf);
            mask.LogList = new uint[] { 0x1098, 0x14ce };
            mask.DiagList = new uint[] { 124 };
            mask.SubSysList = new SubSysMask[] { new SubSysMask(8,1), new SubSysMask(4,15)};
            if (!QXDMProcessor.GetIsf(mask))
            {
                QXDMProcessor.Stop();
                bw.ReportProgress(0, "no valid apdu present.");
                return;
            }

            if (needOTA)
            {
                QXDMProcessor.GetIsfAsync(new LogMask(basePathName + "_qmi.isf",
                    new uint[] { 0x138e, 0x138f, 0x1390, 0x1391, 0x1544 }));

                QXDMProcessor.GetIsfAsync(new LogMask(basePathName + "_ota.isf",
                    new uint[] 
                { 0xb0c0, 0xb0e2, 0xb0e3, 0xb0ec, 0xb0ed, //LTE
                  0x713a, 0x7b3a, 0xd0e3, 0x412f, // UMTS, TDS, W
                  0x1004, 0x1005, 0x1006, 0x1007, 0x1008, //1X
                }));
            }

            bw.ReportProgress(1, "opening apex/qcat");
            string targetText = basePathName + "_apdu.txt";
            if (!IsfAnalyzer.Start(usePCTime))
            {
                bw.ReportProgress(0, "need to install apex or qcat");
                QXDMProcessor.Stop();
                return;
            }

            bw.ReportProgress(1, "saving isf log to text");
            if (!IsfAnalyzer.ConvetIsf2Text(apduIsf, targetText))
            {
                bw.ReportProgress(0, "no valid log present");
                IsfAnalyzer.Stop();
                QXDMProcessor.Stop();
                return;
            }

            IsfAnalyzer.Stop();
            File.Delete(apduIsf);

            string decodedText = basePathName + "_apdu_raw.txt";
            totalCount = GetApduFromText(targetText, decodedText, needSummary);
            
            string message = "total number of extracted apdu is " + totalCount;
            string extra = null;
            if (QXDMProcessor.IsBusy())
            {
                extra = ", still extracting qmi and ota log, do not close";
            }

            bw.ReportProgress(1, message + extra);
            QXDMProcessor.Stop();
            bw.ReportProgress(1, message);
        }
    }
}
