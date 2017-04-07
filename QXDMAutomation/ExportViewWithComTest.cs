using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interop.QXDMLib;

/*
   $Id:  $
   $Header:  $
   $Date: $
   $DateTime: $
   $Change:  $
   $File: $
   $Revision: $
   %Author: katiea $
*/

namespace QXDMAutomation
{
    class ExportViewWithComTest
    {
        static private byte[] response;
        static private bool sentPacket = false; 
        private const int statusRequestCode = 12;

        /* 
         * Request status from phone and store the response
         */
        public static bool Exec(AutomationWindow window)
        {
            // Send request for status
            byte[] request = new byte[1];
            request[0] = statusRequestCode;

            System.Array result = window.SendDmIcdPacketEx(request, 9000);
            System.Threading.Thread.Sleep(4000); 

            // Store response if returned properly
            if (result.Length > 0)
            {
                sentPacket = true;
                response = new byte[result.Length];
                response = ConvertToBytes(result);
            }

            return sentPacket;
        }

        /*
         * If call to sendDmIcdPacketEx was successful, export text from Item View into a store file
         */
        public static bool Teardown(AutomationWindow window)
        {         
            if (sentPacket && response.Length > 0)
            {
                // Get path to export to (current working directory) 
                string storeFilePath = Program.GetWorkingDirectory();
                if (storeFilePath == "")
                    return false;

                storeFilePath += "ExportViewTextWithComTest.txt";

                // Export Item View text to store file
                uint success = window.ExportViewText("Item View", storeFilePath);

                if (success == 0)
                {
                    Console.WriteLine("Unable to export items, 'Item View'");
                    Console.WriteLine("Last error string: " + window.GetLastErrorString());
                    return false;
                }

                Console.WriteLine("Items exported to item store file: " + storeFilePath);
                return true;
            }
            else
            {
                Console.WriteLine("Problem writing or retrieving packet");
                Console.WriteLine("Last error string: " + window.GetLastErrorString());
                return false;
            }
        }

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
