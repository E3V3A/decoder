using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QXDMLib;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using APEX;
using System.ComponentModel;

namespace ProtocolDecoder
{
    class QXDM4Processor : IQXDMProcessor
    {
        private QXDMAutoApplication qxdmApplication = null;
        private AutomationWindow qxdmWindow = null;
        private uint isfHandler = 0xFFFFFFFF;
        private AutomationClientInterface iClient = null;
        private uint clientHandler = 0xFFFFFFFF;
        private AutomationConfigClient iConfig = null;

        public bool Start(string sourceFile)
        {
            qxdmApplication = new QXDMAutoApplication();
            qxdmWindow = qxdmApplication.GetAutomationWindow();

            isfHandler = qxdmWindow.LoadItemStore(sourceFile);
            if (isfHandler == 0xFFFFFFFF)
            {
                Debug.WriteLine("Error:  Failed to load input ISF: {0}", sourceFile);
                return false;
            }
            uint itemCount = qxdmWindow.GetItemCount();
            Debug.WriteLine("itemCount: " + itemCount);

            iClient = (AutomationClientInterface)qxdmWindow.GetClientInterface(isfHandler);
            if (iClient == null)
            {
                Debug.WriteLine("Unable to obtain ISF client interface");
                qxdmWindow.CloseItemStore();
                return false;
            }

            clientHandler = iClient.RegisterClient(true);
            if (clientHandler == 0xFFFFFFFF)
            {
                Debug.WriteLine("Unable to register ISF client");
                qxdmWindow.CloseItemStore();
                return false;
            }

            iConfig = (AutomationConfigClient)iClient.ConfigureClient(clientHandler);
            if (iConfig == null)
            {
                Debug.WriteLine("Unable to configure ISF client");
                iClient.UnregisterClient(clientHandler);
                qxdmWindow.CloseItemStore();
                return false;
            }            
            return true;
        }

        public bool GetIsf(LogMask mask)
        {
            ApplyFilter(mask);

            iConfig.CommitConfig();
            iClient.PopulateClients();

            uint filteredItemCount = iClient.CopyAllClientsItems(mask.TargetFile);

            return (filteredItemCount != 0);
        }

        public void Stop()
        {
            iClient.UnregisterClient(clientHandler);
            qxdmWindow.CloseItemStore();
        }

        private void ApplyFilter(LogMask mask)
        {
            if (mask.MsgList != null && mask.MsgList.Length > 0)
            {
                iConfig.AddItem(6);//Message                
                for (int i = 0; i < mask.MsgList.Length; i++)
                {
                    Debug.WriteLine("{0}", mask.MsgList[i]);
                    iConfig.AddMessage(mask.MsgList[i], 0);
                    iConfig.AddMessage(mask.MsgList[i], 1);
                    iConfig.AddMessage(mask.MsgList[i], 2);
                    iConfig.AddMessage(mask.MsgList[i], 3);
                    iConfig.AddMessage(mask.MsgList[i], 4);
                }
            }
            if (mask.LogList != null && mask.LogList.Length > 0)
            {
                iConfig.AddItem(5);//Log
                for (int i = 0; i < mask.LogList.Length; i++)
                {
                    Debug.WriteLine("0x{0:X}", mask.LogList[i]);
                    iConfig.AddLog(mask.LogList[i]);
                }
            }

        }

    }
}
