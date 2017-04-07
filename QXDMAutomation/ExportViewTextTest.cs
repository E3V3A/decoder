using System;
using System.Globalization;
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
    class ExportViewTextTest
    {
        /* 
         * Load item store file
         */
        public static bool Setup(AutomationWindow window)
        {
            // Get path to ISF file
            string fPath = Program.GetWorkingDirectory();
            if (fPath == "")
                return false;
            fPath += "medium.isf";

            // Get file handle & load item store file
            uint handle = window.LoadItemStore(fPath);
            if (handle == 0)
            {
                Console.Write("\nUnable to load ISF:\n" + fPath + "\n");
                return false;
            }

            return true;
        }

        /*
         * Export Item View text to "ExportViewTextTest.txt" in current working directory
         */
        public static bool Exec(AutomationWindow window)
        {
            string storeFile = Program.GetWorkingDirectory() + "ExportViewTextTest.txt";
            uint success = window.ExportViewText("Item View", storeFile);
            if (success == 0)
            {
                Console.WriteLine("Unable to export items, 'Item View'");
                return false;
            }

            Console.WriteLine("Items exported to item store file: " + storeFile);
            return true;
        }
    }
}
