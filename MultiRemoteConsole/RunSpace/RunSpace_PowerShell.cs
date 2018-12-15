using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiRemoteConsole
{
    class RunSpace_PowerShell : RunSpace_Cmd
    {
        public override string FileName { get; set; } = "powershell.exe";
    }
}
