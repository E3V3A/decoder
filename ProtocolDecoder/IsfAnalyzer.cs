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
    interface IIsfAnalyzer
    {
        bool Start(bool usePCTime);
        void Stop();
        bool ConvetIsf2Text(string isfFile, string txtFile);
    }

    class IsfAnalyzer
    {
        private static IIsfAnalyzer handler = null;
        public static bool Start(bool usePCTime)
        {
            if (Registry.ClassesRoot.OpenSubKey("APEX6.Application") != null)
            {
                handler = new APEXAnalyzer();
                if(handler.Start(usePCTime))
                {
                    return true;
                }
            }
            if (Registry.ClassesRoot.OpenSubKey("QCAT6.Application") != null)
            {
                handler = new APEXAnalyzer();
                if (handler.Start(usePCTime))
                {
                    return true;
                }
            }
            handler = null;
            return false;
        }
        public static void Stop()
        {
            handler.Stop();
        }
        public static bool ConvetIsf2Text(string isfFile, string txtFile)
        {
            return handler.ConvetIsf2Text(isfFile, txtFile);
        }
    }

    class APEXAnalyzer : IIsfAnalyzer
    {
        APEX.Application app = null;
        public bool Start(bool usePCTime)
        {
            if ((app = new APEX.Application()) == null)
            {
                return false;
            }
            app.Visible = 0;
            app.UsePCTime = usePCTime ? 1 : 0;
            return true;
        }

        public void Stop()
        {
            if (app != null)
            {
                app.closeFile();
                app = null;
            }
        }
        public bool ConvetIsf2Text(string isfFile, string txtFile)
        {
            if (app == null || app.Process(isfFile, txtFile, 0, 0) == 0)
            {
                return false;
            }
            return true;
        }
    }

    class QCATAnalyzer : IIsfAnalyzer
    {
        QCAT.Application app = null;
        public bool Start(bool usePCTime)
        {
            if ((app = new QCAT.Application()) == null)
            {
                return false;
            }
            app.Visible = 0;
            app.UsePCTime = usePCTime ? 1 : 0;
            return true;
        }

        public void Stop()
        {
            if (app != null)
            {
                app.closeFile();
                app = null;
            }
        }
        public bool ConvetIsf2Text(string isfFile, string txtFile)
        {
            if (app == null || app.Process(isfFile, txtFile, 0, 0) == 0)
            {
                return false;
            }
            return true;
        }
    }


}
