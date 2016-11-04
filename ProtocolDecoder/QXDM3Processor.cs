using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMCoreAutomation;
using System.Diagnostics;
using System.ComponentModel;

namespace ProtocolDecoder
{
    class QXDM3Processor : IQXDMProcessor
    {
        private ItemStoreFilesClass iIsf = null;
        private uint isfHandler = 0xFFFFFFFF;
        private ISFClient iClient = null;
        private uint clientHandler = 0xFFFFFFFF;
        private IISFClientConfig iConfig = null;

        public bool Start(string sourceFile)
        {
            iIsf = new ItemStoreFilesClass();
            if (iIsf == null)
            {
                Debug.WriteLine("Error:  Failed to initialize ISF interface");
                return false;
            }

            isfHandler = iIsf.LoadItemStore(sourceFile);
            if (isfHandler == 0xFFFFFFFF)
            {
                Debug.WriteLine("Error:  Failed to load input ISF: {0}", sourceFile);
                return false;
            }

            iClient = (ISFClient)iIsf.GetClientInterface(isfHandler);
            if (iClient == null)
            {
                Debug.WriteLine("Unable to obtain ISF client interface");
                iIsf.CloseItemStore(isfHandler);
                return false;
            }

            clientHandler = iClient.RegisterClient(true);
            if (clientHandler == 0xFFFFFFFF)
            {
                Debug.WriteLine("Unable to register ISF client");
                iIsf.CloseItemStore(isfHandler);
                return false;
            }

            iConfig = (IISFClientConfig)iClient.ConfigureClient(clientHandler);
            if (iConfig == null)
            {
                Debug.WriteLine("Unable to configure ISF client");
                iClient.UnregisterClient(clientHandler);
                iIsf.CloseItemStore(isfHandler);
                return false;
            }
            return true;
        }

        public bool GetIsf(Mask mask)
        {
            ApplyFilter(mask);

            iConfig.CommitConfig();
            iClient.PopulateClients();

            return iClient.CopyAllClientsItems(mask.TargetFile);
        }

        public void Stop()
        {
            iClient.UnregisterClient(clientHandler);//如果不去注册，居然会进程异常终止，神奇
            iIsf.CloseItemStore(isfHandler);
        }

        private void ApplyFilter(Mask mask)
        {
            iConfig.ClearConfig();
            if (mask.LogList != null && mask.LogList.Length > 0)
            {
                iConfig.AddItem(5);//Log
                for (int i = 0; i < mask.LogList.Length; i++)
                {
                    iConfig.AddLog(mask.LogList[i]);
                }
            }

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
            if(mask.DiagList!=null && mask.DiagList.Length>0)
            {
                iConfig.AddItem(1);//Diag response
                for(int i =0;i<mask.DiagList.Length;i++)
                {
                    iConfig.AddDIAGResponse(mask.DiagList[i]);
                }
            }
            if (mask.SubSysList != null && mask.SubSysList.Length > 0)
            {
                iConfig.AddItem(9);//sub sys dispatch response
                for (int i = 0; i < mask.SubSysList.Length; i++)
                {
                    iConfig.AddSubsysResponse(mask.SubSysList[i].SubSysID, mask.SubSysList[i].SubSysCmd);
                }
            }
        }
    }
}
