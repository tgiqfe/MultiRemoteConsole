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
        //  コマンドライン引数
        class Arguments
        {
            public string Server { get; set; }
        }

        //  メイン処理
        static void Main(string[] args)
        {
            Arguments arguments = GetArguments(args);

            if (arguments.Server == null)
            {
                WebSocketServer _wss = new WebSocketServer();
                _wss.AcceptURI = "http://*:3000/";
                _wss.Start().Wait();
                return;
            }

            //  カレントディレクトリ
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            WebSocketClient wsc = new WebSocketClient() { TargetURI = arguments.Server };
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

        //  引数
        private static Arguments GetArguments(string[] args)
        {
            Arguments arguments = new Arguments();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/server":
                    case "-server":
                    case "--server":
                    case "/s":
                    case "-s":
                        arguments.Server = args[++i];
                        break;
                }
            }
            return arguments;
        }
    }
}
