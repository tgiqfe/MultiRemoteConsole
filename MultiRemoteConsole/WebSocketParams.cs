using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MultiRemoteConsole
{
    public class WebSocketParams
    {
        //  送受信メッセージのタイプ
        public const string MSG_CONSOLE_CMD = "Message_Console_Cmd";
        public const string MSG_CONSOLE_POWERSHELL = "Message_Console_PowerShell";

        public const string HEADER_STD_IN = "STI:";
        public const string HEADER_STD_OUT = "STO:";
        public const string HEADER_STD_ERR = "ERR:";
        
        //  設定ファイル(XML)に出力するパラメータ
        public string[] AcceptUrls { get; set; }
        public int Port { get; set; }
        public string DirectoryName { get; set; }

        //  コンストラクタ
        public WebSocketParams() { }

        //  初回実行
        public void Init()
        {
            this.AcceptUrls = new string[] { "http://*" };
            this.Port = 3000;
            this.DirectoryName = "/";
        }

        //  デシリアライズ
        public static WebSocketParams Deseiralize(string fileName)
        {
            WebSocketParams wsp = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    switch (Path.GetExtension(fileName))
                    {
                        case ".xml":
                            wsp = new XmlSerializer(typeof(WebSocketParams)).Deserialize(sr) as WebSocketParams;
                            break;
                    }
                }

            }
            catch { }
            if (wsp == null)
            {
                wsp = new WebSocketParams();
                wsp.Init();
            }
            return wsp;
        }

        //  シリアライズ
        public void Serialize(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                switch (Path.GetExtension(fileName))
                {
                    case ".xml":
                        new XmlSerializer(typeof(WebSocketParams)).Serialize(sw, this,
                            new XmlSerializerNamespaces(
                                new XmlQualifiedName[1] { XmlQualifiedName.Empty }));
                        break;
  
                }
            }
        }
    }
}
