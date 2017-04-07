using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interop.QXDMLib;

namespace QXDMAutomation
{
	public class CopyItemsTest
	{
		public static bool Exec(AutomationWindow window)
		{
			Console.Write("\n\nCollect some logs, press enter when QXDM has gathered some.");
			Console.ReadKey(); //wait some time to collect logging  

			return true;
		}

		public static bool Teardown(AutomationWindow window)
		{
			window.CopyViewItems("Item View", "C:\\temp\\CopyViewItems.txt");
			window.ExportViewText("Item View", "C:\\temp\\ExportViewText.txt");

			return true;
		}
	}
}

