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
    public class ProcessMessage
    {
        public static void Run(WebSocket ws, string initMessage)
        {
            switch (initMessage)
            {
                case WebSocketParams.MSG_CONSOLE_CMD:
                    MessageConsoleCmd(ws).Wait();
                    break;
                case WebSocketParams.MSG_CONSOLE_POWERSHELL:
                    MessageConsolePowerShell(ws).Wait();
                    break;
            }
        }

        //  リモートコンソール(CMD)用メッセージ処理部
        private static async Task MessageConsoleCmd(WebSocket ws)
        {
            int headerLength = WebSocketParams.HEADER_STD_IN.Length;
            using (RunSpace_Cmd rs = new RunSpace_Cmd() { Ws = ws })
            {
                ArraySegment<byte> buff = new ArraySegment<byte>(new byte[1024]);
                while (ws.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult ret = await ws.ReceiveAsync(buff, CancellationToken.None);
                    if (ret.MessageType == WebSocketMessageType.Text && ret.Count >= headerLength)
                    {
                        switch (Encoding.UTF8.GetString(buff.Take(headerLength).ToArray()))
                        {
                            case WebSocketParams.HEADER_STD_IN:
                                rs.Input(
                                    Encoding.UTF8.GetString(buff.Skip(headerLength).
                                        Take(ret.Count - headerLength).
                                        ToArray()));
                                break;
                        }
                    }
                }
            }
        }

        //  リモートコンソール(PowerShell)用メッセージ処理部
        private static async Task MessageConsolePowerShell(WebSocket ws)
        {
            int headerLength = WebSocketParams.HEADER_STD_IN.Length;
            using (RunSpace_PowerShell rs = new RunSpace_PowerShell() { Ws = ws })
            {
                ArraySegment<byte> buff = new ArraySegment<byte>(new byte[1024]);
                while (ws.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult ret = await ws.ReceiveAsync(buff, CancellationToken.None);
                    if (ret.MessageType == WebSocketMessageType.Text && ret.Count >= headerLength)
                    {
                        switch (Encoding.UTF8.GetString(buff.Take(headerLength).ToArray()))
                        {
                            case WebSocketParams.HEADER_STD_IN:
                                rs.Input(
                                    Encoding.UTF8.GetString(buff.Skip(headerLength).
                                        Take(ret.Count - headerLength).
                                        ToArray()));
                                break;
                        }
                    }
                }
            }
        }
    }
}
