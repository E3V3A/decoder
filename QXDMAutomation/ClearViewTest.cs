using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interop.QXDMLib;

namespace QXDMAutomation
{
	class ClearViewTestClass
	{
		public static bool Exec(AutomationWindow automationWindow)
		{
			automationWindow.CreateView("Item View", string.Empty);
			System.Threading.Thread.Sleep(10000);
			automationWindow.ClearViewItems("Item View");

			return true;
		}
	}
}
