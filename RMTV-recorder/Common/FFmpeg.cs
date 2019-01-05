using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RMTV_recorder
{
    class FFmpeg
    {
        private Process _process = null;
        //private string _log = string.Empty;
        private List<string> _log = null;

        public FFmpeg()
        {
            _log = new List<string>();
        }

        public void StartRecord(bool isManaul, bool language)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string cmd = String.Concat("-hide_banner -protocol_whitelist \"concat,file,subfile,http,https,tls,rtp,tcp,udp,crypto\" -i \"",
                                       (language? Parameter._m3u8_es_Path : Parameter._m3u8_en_Path),
                                       "\" -c copy ");

            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Parameter._ffmpegPath;
            startInfo.Arguments = string.Concat(cmd, "\"", Parameter._outputPath, "\\", GetOutputFileName(language, isManaul), "\"");
            //startInfo.Arguments = "-h";
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            _process = new Process();
            _process = Process.Start(startInfo);

            //_process.OutputDataReceived += eventOutputDataReceived;
            //_process.BeginOutputReadLine();
            _process.ErrorDataReceived += EventErrorDataReceived;
            _process.BeginErrorReadLine();

        }

        public void StopRecord()
        {
            if (_process != null)
            {
                if(!SendCtrlCKey())
                {
                    _process.Kill();
                }
            }
        }

        public void KillProcess()
        {
            _process.Kill();
        }

        internal const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

        private bool SendCtrlCKey()
        {
            if (AttachConsole((uint)_process.Id))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                        return false;
                    _process.WaitForExit();
                }
                finally
                {
                    FreeConsole();
                    SetConsoleCtrlHandler(null, false);
                }
                return true;
            }
            else
                return false;
        }

        private void EventOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }

        private void EventErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //_log += e.Data + "\n";
            _log.Add(e.Data);
            if (_log.Count > 100)
            {
                _log.RemoveRange(0, _log.Count - 10);
            }
            Debug.WriteLine(e.Data);
        }

        private string GetOutputFileName(bool language, bool isManaul)
        {
            TimeStamp stamp = new TimeStamp();
            return string.Concat("rmtv-record_", 
                                 (language ? "spanish" : "english"), 
                                 "_", 
                                 stamp.GetTimeStamp(), 
                                 "_",
                                 (isManaul ? "manually" : "schedule"),
                                 ".mp4");
        }

        public string GetLog()
        {
            return string.Join("\n", _log.ToArray());
        }

    }
}
