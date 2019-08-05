using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RMTV_recorder
{
    public class FFmpeg
    {
        private Process _process = null;
        private List<string> _log = null;
        private string _fileName = string.Empty;
        private bool _isManaul = true;

        public FFmpeg()
        {
            _log = new List<string>();
        }

        public void StartRecord(string channel)
        {
            _isManaul = true;
            string channellink = "";

            GetChannelLink(channel, ref channellink);
            string arguments = String.Concat("-hide_banner -protocol_whitelist \"concat,file,subfile,http,https,tls,rtp,tcp,udp,crypto\" -i \"",
                                           channellink,
                                           "\"", " -c copy ", 
                                           "\"", Parameter._outputPath, "\\", GetOutputFileName(channel, true, 0), "\"");
            //Debug.WriteLine(arguments);
            RecordVideo(arguments);
        }

        public void StartRecord(string channel, string channellink, int duration, int retry_times)
        {
            _isManaul = false;

            GetChannelLink(channel, ref channellink);
            string arguments = String.Concat("-hide_banner -protocol_whitelist \"concat,file,subfile,http,https,tls,rtp,tcp,udp,crypto\" -i \"",
                               channellink,
                               "\"", " -t ", duration,
                               " -c copy ",
                               "\"", Parameter._outputPath, "\\", GetOutputFileName(channel, false, retry_times), "\"");
            //Debug.WriteLine(arguments);
            RecordVideo(arguments);

            _process.WaitForExit();
        }

        private void RecordVideo(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Parameter._ffmpegPath;
            startInfo.Arguments = arguments;
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
            if (_process != null && !_process.HasExited)
            {
                if(!SendCtrlCKey())
                {
                    //_process.Kill();
                    _log.Add("[Information] Send Ctrl+C failed. -----");
                    Debug.WriteLine("Send Ctrl+C failed.");
                }
                else
                {
                    _log.Add("[Information] Send Ctrl+C successfully. -----");
                }
            }
        }

        public void KillProcess()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }

        public bool CheckAlive()
        {
            return !_process.HasExited;
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

                    System.Threading.Thread.Sleep(300);
                    if (_isManaul)
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
            _log.Add(e.Data);
            if (_log.Count > 100)
            {
                _log.RemoveRange(0, _log.Count - 10);
            }
            Debug.WriteLine(e.Data);
        }

        public bool CheckFIleExist()
        {
            if(File.Exists(Path.Combine(Parameter._outputPath, _fileName)))
            {
                return true;
            }
            return false;
        }

        private void GetChannelLink(string channel, ref string channellink)
        {
            if (channel.Equals(Parameter.Channel_Spanish))
            {
                channellink = Parameter._m3u8_es_Path;
                foreach (M3U8Obj obj in Global._rmtv_link_es)
                {
                    if (CommonFunc.IsFileValid(obj.Path))
                    {
                        channellink = obj.Path;
                        break;
                    }
                }
            }
            else if (channel.Equals(Parameter.Channel_English))
            {
                channellink = Parameter._m3u8_en_Path;
                foreach (M3U8Obj obj in Global._rmtv_link_en)
                {
                    if (CommonFunc.IsFileValid(obj.Path))
                    {
                        channellink = obj.Path;
                        break;
                    }
                }
            }
        }

        private string GetOutputFileName(string channel, bool isManaul, int retry_times)
        {
            TimeStamp stamp = new TimeStamp();
            _fileName =  string.Concat("rmtv-record_", 
                                       stamp.GetTimeStamp(),
                                       "_",
                                       channel.ToLower(),
                                       "_",
                                       (isManaul ? "manual" : "scheduled"),
                                       (retry_times!=0 ? string.Format("(reconnected{0:D2})", retry_times) : ""),
                                       ".mp4");
            return _fileName;
        }

        public string GetLog()
        {
            return string.Join("\n", _log.ToArray());
        }

        public void Dispose()
        {
            if (_process != null)
            {
                _process.Close();
                _process.Dispose();
                _process = null;
            }
        }
    }
}
