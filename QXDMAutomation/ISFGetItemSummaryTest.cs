using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
    class ISFGetItemSummaryTest
    {
        /* 
         * Retrieve item from ISF file in current working directory
         */
        public static bool Setup(AutomationWindow window)
        {
            string path = Program.GetWorkingDirectory();
            if (path == "")
                return false;
            string fileNm = path + "Example.isf";

            // Get file handle & load item store file
            uint handle = window.LoadItemStore(fileNm);
            if (handle == 0) 
            {
                Console.Write("\nUnable to load ISF:\n" + fileNm + "\n");
                return false;
            }

            // Retrieve item
            try
            {
                item = window.GetItem(handle, 2); 
            }
            catch (COMException e)
            {
                Console.WriteLine("Exception while trying to retrieve item: " + e.Message); 
            }

            if (item == null)
            {
                Console.WriteLine("Unable to retrieve item");
                return false;
            }
            return true;
        }

        /*
         * Call GetItemSummary() and print results
         */
        public static bool Exec(AutomationWindow window)
        {
            string summary = item.GetItemSummary();
            if (summary == "")
            {
                Console.WriteLine("Unable to retrieve item summary");
                return false;
            }
            Console.WriteLine("Item summary: " + summary);

            return true;
        }

        private static AutomationColorItem item;
    }
}
