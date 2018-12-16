using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


namespace MultiRemoteConsole
{
    class RunSpace_Cmd : RunSpace
    {
        public override string FileName { get; set; } = "cmd.exe";
        protected int inputLength = 0;
        protected int inputCount = 0;
        protected bool isInit = true;
        protected bool isEnter = false;

        //  標準出力イベント
        protected override void RegisterOutputThread()
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
                            string output = new string(buffer, 0, count);
                            if (isInit)
                            {
                                Output(new ArraySegment<byte>(
                                    Encoding.UTF8.GetBytes(WebSocketParams.HEADER_STD_OUT + output))).Wait();
                                continue;
                            }
                            if (isEnter)
                            {
                                string tempOutput = output.Trim();
                                if (inputLength == 0 && tempOutput == "")
                                {
                                    continue;
                                }
                                inputCount += tempOutput.Length;
                                if (inputCount <= inputLength)
                                {
                                    continue;
                                }
                                else
                                {
                                    isEnter = false;
                                }
                            }
                            Output(new ArraySegment<byte>(
                                Encoding.UTF8.GetBytes(WebSocketParams.HEADER_STD_OUT + output))).Wait();
                        }
                    }
                }
                catch { }
            }, token);
        }

        //  標準エラー出力時イベント
        protected override void RegisterErrorThread()
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
                                WebSocketParams.HEADER_STD_ERR + new string(buffer, 0, count)))).Wait();
                        }
                    }
                }
                catch { }
            }, token);
        }

        //  標準入力コマンドをリダイレクト
        public override void Input(string command)
        {
            command = command.Trim();
            inputLength = command.Length;
            inputCount = 0;
            isInit = false;
            isEnter = true;
            _process.StandardInput.WriteLine(command);
        }
    }
}

