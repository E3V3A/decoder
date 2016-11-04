using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolDecoder
{
    class SubSysMask
    {
        public uint SubSysID = 0;
        public uint SubSysCmd = 0;
        public SubSysMask(uint subSysID, uint subSysCmd)
        {
            SubSysCmd = subSysCmd;
            SubSysID = subSysID;
        }
    }
    class Mask
    {
        public string TargetFile = null;
        public uint[] LogList = null;
        public uint[] MsgList = null;
        public uint[] DiagList = null;
        public SubSysMask[] SubSysList = null;
        public Mask(string targetFile, uint[] loglist = null, uint[] msgList = null, uint[] diagList = null)
        {
            TargetFile = targetFile;
            LogList = loglist;
            MsgList = msgList;
            DiagList = diagList;
        }
    }
}
