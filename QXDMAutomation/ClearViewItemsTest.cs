using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interop.QXDMLib;

namespace QXDMAutomation
{
	public class ClearViewItemsTestClass
	{
		public static bool Setup(AutomationWindow window)
		{
			bool result;
			uint autoResult;

            result = window.LoadConfig(Program.GetWorkingDirectory() + "simple.dmc");
			if (result == true)
			{
				autoResult = window.CloseView("Item View", null);
				Console.WriteLine("Close Itemview, result = " + autoResult);

				autoResult = window.CreateView("Item View", "");
				Console.WriteLine("Create Itemview, result = " + autoResult);
			}
			else
			{
				Console.WriteLine("Load Config Error:" + window.GetLastErrorString());
			}

			return result;
		}

		public static bool Exec(AutomationWindow window)
		{
			Console.Write("\n\nCollect some logs, press enter when QXDM has gathered some.");
			Console.ReadKey(); //wait some time to collect logging  

			return true;
		}

		public static bool Teardown(AutomationWindow window)
		{
			bool result;

			result = window.ClearViewItems("Item View");
			if (result == false)
				Console.WriteLine("Error ClearViewItems: " + window.GetLastErrorString());

			return result;
		}
	}
}

