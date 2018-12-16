using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiRemoteConsole;
using System.IO;
using System.Reflection;

namespace MRC_Client
{
    class Program
    {
        //  メイン処理
        static void Main(string[] args)
        {
            //  カレントディレクトリ
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  引数読み取り
            WebSocketParams wsp = GetArguments(args);

            //  WebSocketサーバ開始
            WebSocketServer _wss = null;
            if (wsp.IsServer && wsp.AcceptUrls.Count > 0)
            {
                //  PATH追加
                if (wsp.AdditinalPath.Length > 0)
                {
                    Environment.SetEnvironmentVariable("PATH",
                        string.Join(";", wsp.AdditinalPath) + ";" +
                        Environment.ExpandEnvironmentVariables("%PATH%"));
                }

                _wss = new WebSocketServer(wsp);
                if (wsp.IsClient)
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
            if (wsp.IsClient && wsp.TargetServer != "")
            {
                WebSocketClient wsc = new WebSocketClient(wsp);
                wsc.ConnectServer().ConfigureAwait(false);
                wsc.SendInit(WebSocketParams.MSG_CONSOLE_CMD).Wait();
                try
                {
                    while (true)
                    {
                        string command = Console.ReadLine();
                        if (command == "exit" || command == "quit")
                        {
                            wsc.SendClose().ConfigureAwait(false);
                            break;
                        }
                        wsc.SendMessage(WebSocketParams.HEADER_STD_IN + command).Wait();

                    }
                }
                catch { }
            }

            //  終了
            if(_wss != null)
            {
                _wss.Stop();
            }
        }

        //  引数
        private static WebSocketParams GetArguments(string[] args)
        {
            WebSocketParams wsp = new WebSocketParams();
            wsp.Init();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/file":
                    case "-file":
                    case "--file":
                    case "/f":
                    case "-f":
                        wsp = WebSocketParams.Deseiralize(args[++i]);
                        break;
                    case "/targetserver":
                    case "-targetserver":
                    case "--targetserver":
                    case "/server":
                    case "-server":
                    case "--server":
                    case "/s":
                    case "-s":
                        wsp.TargetServer = args[++i];
                        wsp.IsClient = true;
                        break;
                    case "/accepturi":
                    case "-accepturi":
                    case "--accepturi":
                    case "/accepturl":
                    case "-accepturl":
                    case "--accepturl":
                    case "/accept":
                    case "-accept":
                    case "--accept":
                    case "/a":
                    case "-a":
                        wsp.AcceptUrls.Add(args[++i]);
                        wsp.IsServer = true;
                        break;
                }
            }
            return wsp;
        }
    }
}
