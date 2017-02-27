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
using System.Windows.Forms;

namespace ProtocolDecoder
{
    class IsfDecoder
    {
        private static long DecodeText(string source)
        {
            string line = null;
            uint index = 0;
            Match match = null;
            StreamReader reader = new StreamReader(source);

            Item item = new Item();
            while ((line = reader.ReadLine()) != null)
            {
                index++;
                match = Regex.Match(line, @"^\d{4} .{6}  (..:..:..\....)  .{4}  0x(....)  (.*)");
                if (match.Success)
                {
                    item.Process();
                    item.Init(index, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                }
                else
                {
                    item.Add(line);
                }
            }
            item.Process();
            item.Close();
            reader.Close();
            return item.GetApduCount();
        }

        public static void DecodeIsf(string sourceIsf, BackgroundWorker bw, bool usePCTime)
        {
            Utils.InitBaseFileName(sourceIsf);
            //FileInfo fileInfo = new FileInfo(isfFile);
            long totalCount = 0;

            if (!Utils.IsValidIsf(sourceIsf))
            {
                MessageBox.Show("choose valid qxdm log");
                return;
            }

            if(bw!=null)
            { 
                bw.ReportProgress(1, "opening qxdm");
            }
            if (!QXDMProcessor.Start(sourceIsf))
            {
                MessageBox.Show("can't open qxdm");
                return;
            }

            if(bw!=null)
            { 
                bw.ReportProgress(1, "extracting isf log");
            }

            LogMask mask = new LogMask(Utils.RawIsfName);
            mask.LogList = new uint[] { 0x1098, 0x14ce, //UIM APDU
                0x1544, 0x138e, 0x138f, 0x1390, 0x1391, 0x1392, 0x1393, 0x1394, 0x1395, 0x1396, 0x1397, 0x1398, 0x1399, 0x139a, 0x139b, 0x139c, 0x139d, 0x139e, 0x139f, 0x13a0, 0x13a1, 0x13a2, 0x13a3, 0x13a4, 0x13a5, 0x13a6, 0x13a7, 0x13a8, 0x13a9, 0x13aa, 0x13ab, 0x13ac, 0x13ad,//QMI
                0xb0c0, 0xb0e2, 0xb0e3, 0xb0ec, 0xb0ed, //OTA LTE
                0x713a, 0x7b3a, 0xd0e3, 0x412f, 0x5b2f, //OTA  UMTS, TDS, W, GSM
                0x1004, 0x1005, 0x1006, 0x1007, 0x1008, //OTA 1X
                0x156e, 0x1830, 0x1831, 0x1832, //IMS
            };
            mask.MsgList = new uint[] { 21, 6039 };


            mask.DiagList = new uint[] { 124 };
            mask.SubSysList = new SubSysMask[] { new SubSysMask(8, 1), new SubSysMask(4, 15) };
            if (!QXDMProcessor.GetIsf(mask))
            {
                QXDMProcessor.Stop();
                MessageBox.Show("no valid log present.");
                return;
            }

            if(bw!=null)
            { 
                bw.ReportProgress(1, "opening apex/qcat");
            }
            if (!IsfAnalyzer.Start(usePCTime))
            {
                MessageBox.Show("need to install apex or qcat");
                QXDMProcessor.Stop();
                return;
            }

            if(bw!=null)
            { 
                bw.ReportProgress(1, "saving isf log to text");
            }
            if (File.Exists(Utils.RawTextName))
            {
                File.Delete(Utils.RawTextName);
            }
            if (!IsfAnalyzer.ConvetIsf2Text(Utils.RawIsfName, Utils.RawTextName))
            {
                MessageBox.Show("no valid log present in extracted isf");
                IsfAnalyzer.Stop();
                QXDMProcessor.Stop();
                return;
            }

            QXDMProcessor.Stop();
            IsfAnalyzer.Stop();
            File.Delete(Utils.RawIsfName);
            
            if(bw!=null)
            { 
                bw.ReportProgress(1, "decoding text file");
            }
            totalCount = DecodeText(Utils.RawTextName);

            string message = "total number of extracted apdu is " + totalCount;

            if(bw!=null)
            { 
                bw.ReportProgress(1, message);
            }
        }
    }
}
