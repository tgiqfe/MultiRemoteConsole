using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MultiRemoteConsole
{
    public class WebSocketServer
    {
        //  公開パラメータ
        public string AcceptURI { get; set; } = "http://localhost:3000/";

        //  プライベートパラメータ
        private HttpListener httpListener = null;

        //  コンストラクタ
        public WebSocketServer() { }

        //  待ち受け開始
        public async Task Start()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(AcceptURI);
            httpListener.Start();
            while (true)
            {
                HttpListenerContext listenerContext = await httpListener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(listenerContext);
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
        }

        //  リクエスト受け付け時処理
        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            using (WebSocket ws = (await listenerContext.AcceptWebSocketAsync(subProtocol: null)).WebSocket)
            {
                try
                {
                    ArraySegment<byte> buff = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult ret = await ws.ReceiveAsync(buff, CancellationToken.None);
                    if (ret.MessageType == WebSocketMessageType.Text)
                    {
                        ProcessMessage.Run(ws, Encoding.UTF8.GetString(buff.Take(ret.Count).ToArray()));
                    }
                }
                catch { }
            }
        }

        //  終了処理
        public void Stop()
        {
            httpListener.Close();
        }
    }
}
