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
        public string TargetServer { get; set; }
        public List<string> AcceptUrls { get; set; }
        public string[] AddPath { get; set; }
        public string Execute { get; set; }
        public bool IsClient { get; set; }
        public bool IsServer { get; set; }

        //  コンストラクタ
        public WebSocketParams() { }

        //  初回実行
        public void Init()
        {
            this.TargetServer = "";
            this.AcceptUrls = new List<string>();
            this.AddPath = new string[0];
            this.Execute = "cmd.exe";
            this.IsClient = false;
            this.IsServer = false;
        }

        //  デシリアライズ
        public static WebSocketParams Deseiralize(string fileName)
        {
            WebSocketParams wsp = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    wsp = new XmlSerializer(typeof(WebSocketParams)).Deserialize(sr) as WebSocketParams;
                }
            }
            catch { }
            if (wsp == null)
            {
                wsp = new WebSocketParams();
                wsp.Init();
                wsp.Serialize(fileName);
            }
            return wsp;
        }

        //  シリアライズ
        public void Serialize(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                new XmlSerializer(typeof(WebSocketParams)).Serialize(sw, this,
                    new XmlSerializerNamespaces(
                        new XmlQualifiedName[1] { XmlQualifiedName.Empty }));
            }
        }
    }
}
