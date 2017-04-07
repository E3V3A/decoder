// Confidential and Proprietary – Qualcomm Technologies, Inc.

// NO PUBLIC DISCLOSURE PERMITTED:  Please report postings of this software on public servers or websites
// to: DocCtrlAgent@qualcomm.com.

// RESTRICTED USE AND DISCLOSURE:
// This software contains confidential and proprietary information and is not to be used, copied, reproduced, modified
// or distributed, in whole or in part, nor its contents revealed in any manner, without the express written permission
// of Qualcomm Technologies, Inc.

// Qualcomm is a trademark of Qualcomm Technologies Incorporated, registered in the United States and other countries. All
// Qualcomm Technologies Incorporated trademarks are used with permission.

// This software may be subject to U.S. and international export, re-export, or transfer laws.  Diversion contrary to U.S.
// and international law is strictly prohibited.

// Qualcomm Technologies, Inc.
// 5775 Morehouse Drive
// San Diego, CA 92121 U.S.A.
// Copyright © 2013-2016 Qualcomm Technologies, Inc.
// All rights reserved.
// Qualcomm Technologies Confidential and Proprietary

/*
   $Id:  $
   $Header:  $
   $Date: $
   $DateTime: $
   $Change:  $
   $File: $
   $Revision: $
   %Author: msimpson $
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using Interop.QXDMLib;

using TestFunctions = System.Collections.Generic.Dictionary<string, QXDMAutomation.TestCase>;
using TestFunctionsIterator = System.Collections.Generic.Dictionary<string, QXDMAutomation.TestCase>.Enumerator;
using CurrTestFunction = System.Collections.Generic.KeyValuePair<string, QXDMAutomation.TestCase>;

namespace QXDMAutomation
{
	class TestCase
	{
		public TestCase
		(
			bool requiresComPort,
			QXDMAutomation.Program.TestFunctionDelegate setup,
			QXDMAutomation.Program.TestFunctionDelegate exec,
			QXDMAutomation.Program.TestFunctionDelegate teardown
		)
		{
			_requiresComPort = requiresComPort;
			_setup = setup;
			_exec = exec;
			_teardown = teardown;
		}

		public bool _requiresComPort;
		public QXDMAutomation.Program.TestFunctionDelegate _setup;
		public QXDMAutomation.Program.TestFunctionDelegate _exec;
		public QXDMAutomation.Program.TestFunctionDelegate _teardown;
	}

	class Program
	{
		// Program exit error codes
		public enum ExitCode
		{
			SUCCESS = 0,
			AUTOMATION_SETUP_ERROR = -1,
			TESTCASE_FAILED = -2,
			MULT_TESTCASES_FAILED = -3,
		}

		// Command line args
		private const string waitForInput_arg = "nowait";  // Exit immediately rather than waiting for user input
		private const string runall = "runall";            // Run all test cases


		public delegate bool TestFunctionDelegate(AutomationWindow automationWindow);

		static TestFunctions gTestFunctions = new TestFunctions();

		private static System.IO.StreamWriter logFile;
		private static bool waitForInputToExit;

		static void Main(string[] args)
		{
			string functionName = null;
			uint port = 0;
			int numFailed = 0;

			// Initialize log file
			logFile = new System.IO.StreamWriter(GetWorkingDirectory() + "log.txt");
			logFile.WriteLine("QXDM Automation Test Log");

			// Parse arguments
			waitForInputToExit = true;
			switch (args.Length)
			{
				case 0:
					ExitWithFail("Tried to run with improper syntax. Syntax is: QXDMAutomation <functionname> {port #} {nowait}", ExitCode.AUTOMATION_SETUP_ERROR);
					break;

				case 1:
					functionName = args[0];
					break;

				// 2nd argument is ambiguous (port or "nowait"?) so check by type
				case 2:
					functionName = args[0];

					uint tempPort = 0;
					if (uint.TryParse(args[1], out tempPort))
						port = tempPort;
					else if (args[1] == waitForInput_arg)
						waitForInputToExit = false;

					break;

				default:
					functionName = args[0];
					uint.TryParse(args[1], out port);
					if (args[2] == waitForInput_arg)
						waitForInputToExit = false;
					break;
			}

			if (InitializeTestFunctions() == false)
			{
				ExitWithFail("Could not initialize test functions.", ExitCode.AUTOMATION_SETUP_ERROR);
			}

			QXDMAutoApplication qxdmApplication = null;
			try
			{
				qxdmApplication = (QXDMAutoApplication)System.Runtime.InteropServices.Marshal.GetActiveObject("QXDM.QXDMAutoApplication");
			}
			catch (COMException e)
			{
				if (e.HResult == -2147221021)						// Running object not found
					qxdmApplication = new QXDMAutoApplication();	// Attempt to start it

				if (qxdmApplication == null)
				{
					ExitWithFail("Error Starting QXDM", ExitCode.AUTOMATION_SETUP_ERROR);
				}
			}

			if (qxdmApplication != null)
			{
				Console.WriteLine("Interface Version: {0}", qxdmApplication.Get_Automation_Version());
				AutomationWindow automationWindow = qxdmApplication.GetAutomationWindow();
				automationWindow.SetVisible(true);

				Console.WriteLine("QXDM Version: {0}", automationWindow.GetQXDMVersion());

				QXDMAutomation.TestCase testCase = null;

				if (functionName.ToLower() == runall) // Running all test cases
				{
					TestFunctionsIterator iterator = gTestFunctions.GetEnumerator();
					while (iterator.MoveNext() == true)
					{
						CurrTestFunction current = iterator.Current;
						string testName = current.Key;
						testCase = current.Value;

						// If no port is defined, only run test cases that do not require a COM port
						if (port != 0 || testCase._requiresComPort == false)
						{
							if (RunTestCase(testCase, testName, automationWindow, port) == false)
								numFailed++;
						}
					}
				}
				else if (gTestFunctions.TryGetValue(functionName.ToLower(), out testCase)) // Run a single test case
				{
					if (RunTestCase(testCase, functionName, automationWindow, port) == false)
						numFailed++;
				}
				else
				{
					LogEverywhere("Function " + functionName + " not found."); // Could not find test case
					numFailed++;
				}

				automationWindow.Quit();

				// Exit with an error if any test cases failed.
				if (numFailed > 1)
					ExitWithFail("Two or more test cases failed.", ExitCode.MULT_TESTCASES_FAILED);
				else if (numFailed == 1)
					ExitWithFail("A test case failed.", ExitCode.TESTCASE_FAILED);

				LogEverywhere("\n. . .\n[SUCCESS] All test cases passed");

				if (waitForInputToExit)
				{
					Console.Write("\n\nPress enter to exit.");
					Console.Read();
				}
			}

			logFile.Close();
		}

		static bool RunTestCase(QXDMAutomation.TestCase testCase, string functionName, AutomationWindow automationWindow, uint port)
		{
			bool usesComPort = testCase._requiresComPort;

			// Print to log
			string msg = "< RUNNING TEST CASE: " + functionName.ToUpper() + " >";
			LogEverywhere("");

			for (int i = 0; i < msg.Length; i++)
			{
				Console.Write("\\");
				logFile.Write("\\");
			}
			LogEverywhere("\n" + msg);

			string errorMessage = "\t[ERROR] Test case \"" + functionName + "\" failed: ";
			string successMessage = "\t[PASS] Test case \"" + functionName + "\" succeeded";

			// Run setup function
			if (testCase._setup != null && testCase._setup(automationWindow) == false)
			{
				logFile.WriteLine(errorMessage + "Setup function failed");
				return false;
			}

			// Set up COM stuff if needed
			if (usesComPort == true)
			{
				if (port != 0)
				{
					if (automationWindow.SetComPort(port) == true)
					{
						if (automationWindow.GetServerState() == 2)
						{
							Console.WriteLine("Connected to port: {0}", port);
						}
						else
						{
							logFile.WriteLine(errorMessage + "Error connecting to port: " + port);
							return false;
						}
					}
					else
					{
						logFile.WriteLine(errorMessage + "SetComPort Failed - port: " + port + " message: " + automationWindow.GetLastErrorString());
						return false;
					}
				}
				else
				{
					logFile.WriteLine(errorMessage + "Requires a connection");
					return false;
				}
			}

			// Run exec function
			if (testCase._exec != null && testCase._exec(automationWindow) == false)
			{
				logFile.WriteLine(errorMessage + "Exec function failed");
				return false;
			}

			// Disconnect the port if in use
			if (usesComPort == true)
				automationWindow.SetComPort(0);

			// Run teardown (cleanup) function
			if (testCase._teardown != null && testCase._teardown(automationWindow) == false)
			{
				logFile.WriteLine(errorMessage + "Teardown function failed");
				return false;
			}

			logFile.WriteLine(successMessage);
			return true;
		}

		static bool InitializeTestFunctions()
		{
			gTestFunctions["ClearViewItemsTest".ToLower()] = new TestCase(true, ClearViewItemsTestClass.Setup,
				  ClearViewItemsTestClass.Exec, ClearViewItemsTestClass.Teardown);

			gTestFunctions["ClearViewTest".ToLower()] = new TestCase(true, null, ClearViewTestClass.Exec, null);
			gTestFunctions["CopyItemsTest".ToLower()] = new TestCase(true, null, CopyItemsTest.Exec, CopyItemsTest.Teardown);
			gTestFunctions["GetSummaryTest".ToLower()] = new TestCase(false, null, GetSummaryTestClass.Exec, null);
			gTestFunctions["RegisterClientTest".ToLower()] = new TestCase(true, null, RegisterClientTestClass.Exec, null);
			gTestFunctions["SendDmIcdPacketExTest".ToLower()] = new TestCase(true, null, SendDmIcdPacketExTest.Exec, SendDmIcdPacketExTest.Teardown);
			gTestFunctions["ISFGetItemSummaryTest".ToLower()] = new TestCase(false, ISFGetItemSummaryTest.Setup, ISFGetItemSummaryTest.Exec, null);
			gTestFunctions["ExportViewTextTest".ToLower()] = new TestCase(false, ExportViewTextTest.Setup, ExportViewTextTest.Exec, null);
			gTestFunctions["ExportViewWithComTest".ToLower()] = new TestCase(true, null, ExportViewWithComTest.Exec, ExportViewWithComTest.Teardown);

			return gTestFunctions.Count > 0;
		}

		static void LogEverywhere(string errorMessage)
		{
			logFile.WriteLine(errorMessage);
			Console.WriteLine(errorMessage);
		}

		public static void ExitWithFail(string errorMsg, ExitCode exitCode)
		{
			Console.WriteLine("");
			LogEverywhere("\n. . .\n[FINISHED EXECUTING WITH ERRORS] " + errorMsg);
			logFile.Close();

			if (waitForInputToExit)
			{
				Console.Write("\n\nPress enter to exit.");
				Console.Read();
			}

			Environment.Exit((int)exitCode);
		}

		public static string GetWorkingDirectory()
		{
			string path = Directory.GetCurrentDirectory();
			if (path.Length <= 0)
			{
				Console.WriteLine("Unable to get path");
				return "";
			}

			// Make sure '\' at end of path
			if (path[path.Length - 1] != '\\')
				path += '\\';

			return path;
		}
	}


}

