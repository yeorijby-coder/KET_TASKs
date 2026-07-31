using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Samoh_Socket;
using Microsoft.VisualBasic;
using System.Collections;
using System.Xml;
using System.IO;

namespace TSK_COMM_IOSCH
{
    public class cWork
    {
        private bool _bConnet = false;
        public Samoh_Socket.SocketServer.CServiceClient _MyClient;

        public byte[] _byRxBuff;
        public byte[] _byTxBuff;

        public void Connect()
        {
            _bConnet = true;
        }

        public void DisConnect()
        {
            _bConnet = false;
        }
        public void ClearBuff()
        {
            _byRxBuff = new byte[1024];
            _byTxBuff = new byte[1024];
            Array.Clear(_byRxBuff, 0, _byRxBuff.Length);
            Array.Clear(_byTxBuff, 0, _byRxBuff.Length);
        }

        public bool IsConnect
        {
            get
            {
                return _bConnet;
            }
        }

        public void XmlGetRootChildNodes(ref ArrayList argElemet, ref ArrayList argVal, string argXmlStr)
        {
            StringReader strRead = new StringReader(argXmlStr);
            XmlTextReader xmlread = new XmlTextReader(strRead);

            argElemet.Clear();
            argVal.Clear();

            while (xmlread.Read())
            {
                if (xmlread.NodeType == XmlNodeType.Element)
                {
                    argElemet.Add(xmlread.Name);

                    if (xmlread.AttributeCount > 0)
                    {
                        ArrayList ArrSub = new ArrayList();
                        for (int i = 0 ; i <= xmlread.AttributeCount - 1;i++)
                        {
                            ArrSub.Add(xmlread[i]);
                        }
                        argVal.Add(ArrSub);
                    }
                    else
                    {
                        argVal.Add(null);
                    }
                }
                else if (xmlread.NodeType == XmlNodeType.Text)
                {
                    argVal.Add(xmlread.Value);
                }
            }
        }

        public void  ByteChk(out string[] argRetstr, string argSrcstr)
        {
            char STX;
            STX = (char)0x02;
            char ETX;
            ETX = (char)0x03;

            int offset = 0;
            int index = 0;
            int[] offsets = new int[argSrcstr.Length + 1];

            while (index < argSrcstr.Length)
            {
                int indexOf = argSrcstr.IndexOf(STX, index);
                if (indexOf != -1)
                {
                    offsets[offset++] = indexOf;
                    index = (indexOf + 1);
                }
                else
                {
                    index = argSrcstr.Length;
                }
            }

            string[] final = new string[offset+1];

            if (offset == 0)
            {
                final[0] = argSrcstr;
            }
            else
            {
                offset--;
                final[0] = argSrcstr.Substring(0, offsets[0]);
                for (int i = 0; i < offset; i++)
                {
                    final[i + 1] = argSrcstr.Substring(offsets[i] + 1, offsets[i + 1] - offsets[i] - 1);
                }
                final[offset + 1] = argSrcstr.Substring(offsets[offset] + 1);
            }

            for (int i = 0; i <= final.Length  - 1; i++)
            {
                int indexOf = final[i].IndexOf(ETX, 0);
                if (indexOf != -1)
                {
                    final[i] = final[i].Substring(0, indexOf);
                }
            }
            argRetstr = final;
        }
    }

}
