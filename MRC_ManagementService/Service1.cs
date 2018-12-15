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

namespace MRC_ManagementService
{
    public partial class Service1 : ServiceBase
    {
        //  クラスパラメータ
        WebSocketServer _wss = null;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _wss = new WebSocketServer();
            _wss.AcceptURI = "http://*:3000/";
            _wss.Start().ConfigureAwait(false);
        }

        protected override void OnStop()
        {
            _wss.Stop();
        }
    }
}
