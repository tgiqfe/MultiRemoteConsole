using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using MultiRemoteConsole;
using System.IO;
using System.Reflection;

namespace MRC_ManagementService
{
    public partial class Service1 : ServiceBase
    {
        //  静的パラメータ
        const string CONF_FILE = "MRC.xml";

        //  クラスパラメータ
        WebSocketServer _wss = null;

        public Service1()
        {
            InitializeComponent();
        }

        //  サービス開始
        protected override void OnStart(string[] args)
        {
            //  カレントディレクトリ
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  設定ファイル読み込み
            WebSocketParams wsp = WebSocketParams.Deseiralize(CONF_FILE);

            //  PATH追加
            if (wsp.AddPath.Length > 0)
            {
                Environment.SetEnvironmentVariable("PATH",
                    string.Join(";", wsp.AddPath.Select(x => Path.GetFullPath(x))) + ";" +
                    Environment.ExpandEnvironmentVariables("%PATH%"));
            }

            //  WebSocketサーバ開始
            _wss = new WebSocketServer(wsp);
            _wss.Start().ConfigureAwait(false);
        }

        //  サービス終了
        protected override void OnStop()
        {
            _wss.Stop();
        }
    }
}
