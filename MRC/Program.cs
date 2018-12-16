using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiRemoteConsole;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MRC_Client
{
    class Program
    {
        //  コマンドライン引数
        class Arguments
        {
            public string Uri { get; set; }
            public bool IsServer { get; set; }
            public bool IsClient { get; set; }
        }

        //  メイン処理
        static void Main(string[] args)
        {
            //  カレントディレクトリ
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  引数読み取り
            Arguments arguments = GetArguments(args);

            //  WebSocketサーバ開始
            if (arguments.IsServer && arguments.Uri != null)
            {
                WebSocketServer _wss = new WebSocketServer();
                _wss.AcceptURI = arguments.Uri;
                if (arguments.IsClient)
                {
                    _wss.Start().ConfigureAwait(false);
                }
                else
                {
                    _wss.Start().Wait();
                    return;
                }
            }

            //  WebSocketサーバへ接続
            if (arguments.IsClient && arguments.Uri != null)
            {
                WebSocketClient wsc = new WebSocketClient() { TargetURI = arguments.Uri };
                wsc.ConnectServer().ConfigureAwait(false);
                wsc.SendInit(WebSocketParams.MSG_CONSOLE_CMD).Wait();
                try
                {
                    while (true)
                    {
                        string command = Console.ReadLine();
                        wsc.SendMessage(WebSocketParams.HEADER_STD_IN + command).Wait();
                    }
                }
                catch { }
            }
        }

        //  引数
        private static Arguments GetArguments(string[] args)
        {
            Arguments arguments = new Arguments();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/uri":
                    case "-uri":
                    case "--uri":
                    case "/url":
                    case "-url":
                    case "--url":
                    case "/u":
                    case "-u":
                        arguments.Uri = args[++i];
                        break;
                    case "server":
                    case "/server":
                    case "-server":
                    case "--server":
                    case "/s":
                    case "-s":
                        arguments.IsServer = true;
                        break;
                    case "client":
                    case "/client":
                    case "-client":
                    case "--client":
                    case "/c":
                    case "-c":
                        arguments.IsClient = true;
                        break;
                }
            }
            return arguments;
        }
    }
}
