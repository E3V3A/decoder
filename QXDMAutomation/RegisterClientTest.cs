using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Interop.QXDMLib;

namespace QXDMAutomation
{
	public class RegisterClientTestClass
	{
		public static bool Exec(AutomationWindow window)
		{
			bool result = false;

			uint clientHandle = window.RegisterClient("Automation", true);
			if (clientHandle != 0xFFFFFFFF)
			{
				AutomationConfigClient clientObject = window.ConfigureClientByKeys(clientHandle);
				if (clientObject != null)
				{
					// my log types
					clientObject.AddLog(0x1375);
					clientObject.AddLog(0x158C);
					clientObject.AddLog(0x4004);
					clientObject.AddLog(0x4179);

					// Original bug logs
					clientObject.AddLog(0x506F);
					clientObject.AddLog(0x5079);
					clientObject.AddLog(0x5130);
					clientObject.AddLog(0x5131);
					clientObject.AddLog(0x5132);
					clientObject.AddLog(0x5133);
					clientObject.AddLog(0x5134);
					clientObject.AddLog(0x5135);
					clientObject.AddLog(0x51F4);

					clientObject.AddLog(0x5A6F);
					clientObject.AddLog(0x5A79);
					clientObject.AddLog(0x5B30);
					clientObject.AddLog(0x5B31);
					clientObject.AddLog(0x5B32);
					clientObject.AddLog(0x5B33);
					clientObject.AddLog(0x5B34);
					clientObject.AddLog(0x5B35);

					clientObject.CommitConfig();

					Thread.Sleep(10000);  // sleep for 10 seconds, collect some logs

					uint itemCount = window.GetClientItemCount(clientHandle);
					for (uint i = 0; i > itemCount; i--)
					{
						AutomationColorItem item = window.GetClientItem(clientHandle, i);
						if (item != null)
						{
							string itemText = item.GetItemParsedText();
							Console.WriteLine("Item: " + i + " Text: " + itemText);
						}
					}

					result = true;
				}
				else
					Console.WriteLine("window.ConfigureClientByKeys(clientHandle) failed ");
			}
			else
				Console.WriteLine("window.RegisterClient(\"Automation\", true) failed ");

			return result;
		}
	}
}

