using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace MultiRemoteConsole
{
    public class WebSocketClient
    {
        //  静的パラメータ
        const int TRY_OPENWAIT = 50;
        const int TRY_INTERVAL = 100;

        //  プライベートパラメータ
        private WebSocketParams _wsp = null;
        private ClientWebSocket _cws = new ClientWebSocket();

        //  コンストラクタ
        public WebSocketClient() { }
        public WebSocketClient(WebSocketParams wsp)
        {
            this._wsp = wsp;
        }

        //  サーバ接続開始
        public async Task ConnectServer()
        {
            await _cws.ConnectAsync(new Uri(_wsp.TargetServer), CancellationToken.None);
            ArraySegment<byte> buff = new ArraySegment<byte>(new byte[1024 * 1024]);
            int headerLength = WebSocketParams.HEADER_STD_OUT.Length;

            while (_cws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult ret = await _cws.ReceiveAsync(buff, CancellationToken.None);
                if (ret.MessageType == WebSocketMessageType.Text && ret.Count > headerLength)
                {
                    switch (Encoding.UTF8.GetString(buff.Take(headerLength).ToArray()))
                    {
                        case WebSocketParams.HEADER_STD_OUT:
                            Console.Write(Encoding.UTF8.GetString(buff.Skip(headerLength).
                                Take(ret.Count - headerLength).
                                ToArray()));
                            break;
                        case WebSocketParams.HEADER_STD_ERR:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(Encoding.UTF8.GetString(buff.Skip(headerLength).
                                Take(ret.Count - headerLength).
                                ToArray()));
                            Console.ResetColor();
                            break;
                    }
                }
            }
        }

        //  Initメッセージ送信
        public async Task SendInit(string initMessage)
        {
            for (int i = 0; _cws.State != WebSocketState.Open && i < TRY_OPENWAIT; i++)
            {
                Thread.Sleep(TRY_INTERVAL);
            }
            await _cws.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(initMessage)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        //  メッセージ送信
        public async Task SendMessage(string message)
        {
            await _cws.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

        }
        public async Task SendMessage(ArraySegment<byte> messageArraySegment)
        {
            await _cws.SendAsync(
                messageArraySegment,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        //  Closeメッセージ送信
        public async Task SendClose()
        {
            await _cws.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
    }
}
