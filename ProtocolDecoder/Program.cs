using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ProtocolDecoder
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Args.Length == 0)
            {
                Application.Run(new Form1());
            }
            else if (Args.Length == 1)
            {

                IsfDecoder.DecodeIsf(Args[0], null, false);

            }
            else if (Args.Length == 2)
            {
                
            }
        }
    }
}
