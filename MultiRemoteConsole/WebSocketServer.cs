using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiRemoteConsole
{
    public class WebSocketServer
    {
        //  クラスパラメータ
        private WebSocketParams _wsp = null;
        private HttpListener _httpListener = null;

        //  コンストラクタ
        public WebSocketServer() { }
        public WebSocketServer(WebSocketParams wsp)
        {
            this._wsp = wsp;
        }

        //  待ち受け開始
        public async Task Start()
        {
            _httpListener = new HttpListener();
            foreach (string acceptUri in _wsp.AcceptUrls)
            {
                if (acceptUri.EndsWith("/"))
                {
                    _httpListener.Prefixes.Add(acceptUri);
                }
                else
                {
                    _httpListener.Prefixes.Add(acceptUri + "/");
                }
            }
            _httpListener.Start();
            while (true)
            {
                HttpListenerContext listenerContext = await _httpListener.GetContextAsync();
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
            if (_httpListener.IsListening)
            {
                _httpListener.Close();
            }
        }
    }
}
