using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace MultiRemoteConsole
{
    class RunSpace : IDisposable
    {
        //  制御用パラメータ
        protected Process _process;
        protected StringBuilder _outputBuffer = new StringBuilder("", 10 * 1024 * 1024);
        protected CancellationTokenSource outputTokenSource = new CancellationTokenSource();
        protected CancellationTokenSource errorTokenSource = new CancellationTokenSource();
        protected virtual int _bufferSize { get; set; } = 1024 * 1024;

        public virtual string FileName { get; set; } = "cmd.exe";
        public virtual string Arguments { get; set; } = "";
        public virtual WebSocket Ws { get; set; }

        //  コンストラクタ
        public RunSpace()
        {
            //  プロセス開始
            CreateProcess();

            //  標準出力時イベントを開始
            RegisterOutputThread();

            //  標準エラー出力時イベント
            RegisterErrorThread();
        }

        //  プロセス開始用メソッド
        protected virtual void CreateProcess()
        {
            _process = new Process();
            _process.StartInfo.FileName = FileName;
            _process.StartInfo.Arguments = Arguments;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.Start();
            _process.EnableRaisingEvents = true;
            _process.Exited += new EventHandler(CloseEvent);
        }

        //  終了時イベント
        protected virtual void CloseEvent(object sender, EventArgs e)
        {
            Dispose();
        }

        //  標準出力イベント
        protected virtual void RegisterOutputThread()
        {
            CancellationToken token = outputTokenSource.Token;
            Task.Run(() =>
            {
                char[] buffer = new char[_bufferSize];
                try
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested) { break; }
                        int count = _process.StandardOutput.Read(buffer, 0, buffer.Length);
                        lock (_outputBuffer)
                        {
                            _outputBuffer.Append(buffer, 0, count);
                            Output(new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                                    WebSocketParams.HEADER_STD_OUT + new string(buffer, 0, count)))).Wait();
                        }
                    }
                }
                catch { }
            }, token);
        }

        //  標準エラー出力時イベント
        protected virtual void RegisterErrorThread()
        {
            CancellationToken token = errorTokenSource.Token;
            Task.Run(() =>
            {
                char[] buffer = new char[_bufferSize];
                try
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested) { break; }
                        int count = _process.StandardError.Read(buffer, 0, buffer.Length);
                        lock (_outputBuffer)
                        {
                            _outputBuffer.Append(buffer, 0, count);
                            Output(new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                                    WebSocketParams.HEADER_STD_OUT + new string(buffer, 0, count)))).Wait();
                        }
                    }
                }
                catch { }
            }, token);
        }

        //  標準出力/標準エラー出力をリダイレクト
        protected virtual async Task Output(ArraySegment<byte> output)
        {
            if (Ws != null && Ws.State == WebSocketState.Open)
            {
                await Ws.SendAsync(output, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        //  標準入力コマンドをリダイレクト
        public virtual void Input(string command)
        {
            _process.StandardInput.WriteLine(command.Trim());
        }

        //  Dipose
        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    outputTokenSource.Cancel();
                    errorTokenSource.Cancel();
                    _process.Close();
                    Console.ResetColor();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
