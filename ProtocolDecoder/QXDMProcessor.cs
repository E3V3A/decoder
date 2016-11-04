using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;

namespace ProtocolDecoder
{
    interface IQXDMProcessor
    {
        bool Start(string file);
        bool GetIsf(LogMask mask);
        void Stop();
    }

    class QXDMProcessor
    {
        private static IQXDMProcessor qxdm = null;
        private static Thread workThread = null;
        private static Queue<LogMask> maskQueue = null;
        public static bool Start(string sourceFile)
        {
            workThread = new Thread(GetIsfVoid);
            maskQueue = new Queue<LogMask>();

            bool qxdm3Present = (Registry.ClassesRoot.OpenSubKey("QXDM.Application") != null);
            bool qxdm4Present = (Registry.ClassesRoot.OpenSubKey("QXDM.QXDMAutoApplication") != null);

            if (qxdm3Present)
            {
                qxdm = new QXDM3Processor();
                if (qxdm.Start(sourceFile) == true)
                {
                    return true;
                }
            }
            if (qxdm4Present)
            {
                qxdm = new QXDM4Processor();
                if (qxdm.Start(sourceFile) == true)
                {
                    return true;
                }
            }

            qxdm = null;
            return false;
        }
        public static bool GetIsf(LogMask mask)
        {
            if (qxdm != null)
            {
                return qxdm.GetIsf(mask);
            }

            return false;
        }

        private static void GetIsfVoid()
        {
            while (maskQueue.Count > 0)
            {
                GetIsf(maskQueue.Peek());
                maskQueue.Dequeue();
            }
        }


        public static void GetIsfAsync(LogMask mask)
        {
            lock (maskQueue)
            {
                maskQueue.Enqueue(mask);
                if (!workThread.IsAlive)
                {
                    workThread.Start();
                }
            }
        }

        public static void Stop()
        {
            if (workThread.IsAlive)
            {
                Debug.WriteLine("wait child thread");
                workThread.Join();
                Debug.WriteLine("child child end");
            }
            if (qxdm != null)
            {
                qxdm.Stop();
                qxdm = null;
            }

        }

        public static bool IsBusy()
        {
            return workThread.IsAlive;
        }
    }
}
