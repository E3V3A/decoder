using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interop.QXDMLib;

namespace QXDMAutomation
{
	public class GetSummaryTestClass
	{
		public static bool Exec(AutomationWindow window)
		{
			//uint id = 1820;
			uint id = 1605;
			byte[] payload;

			//payload = new byte[9]; // works
			//payload = new byte[2]; // does not work
			//for (int index = 0; index < payload.Length; index++)
			//   payload[index] = 0;

			payload = new byte[10];
			payload[0] = 0x02;
			payload[1] = 0x00;
			payload[2] = 0x00;
			payload[3] = 0x00;
			payload[4] = 0x00;
			payload[5] = 0x00;
			payload[6] = 0x00;
			payload[7] = 0x00;
			payload[8] = 0x00;
			payload[9] = 0x02;

			string result = window.GetSummary(id, payload);
			Console.WriteLine("Result:" + result);

			return true;
		}
	}
}

