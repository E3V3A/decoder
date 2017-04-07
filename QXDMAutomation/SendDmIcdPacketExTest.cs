using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interop.QXDMLib;

namespace QXDMAutomation
{
	public class SendDmIcdPacketExTest
	{
		public static bool Exec(AutomationWindow window)
		{
			byte[] payload;

			payload = new byte[1];
			payload[0] = 12;

			System.Array result;

			result = window.SendDmIcdPacketEx(payload, 9000);
			if (result.Length > 0)
			{
				_payload = new byte[result.Length];
				_payload = ConvertToBytes(result);
			}

			return true;
		}

		public static bool Teardown(AutomationWindow window)
		{
			if (_payload.Length > 0)
				System.Console.WriteLine(_payload);
			else
				System.Console.WriteLine("Payload Empty");

			return true;
		}

		static private byte[] _payload;

		static private byte[] ConvertToBytes(System.Array myArray)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(ms);
            foreach (object obj in myArray)
                sw.Write(obj);
            
            sw.Flush();

            return ms.GetBuffer();
        }
	}
}

