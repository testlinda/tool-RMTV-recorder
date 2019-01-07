using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RMTV_recorder
{
    class Parameter
    {
        public static string _version = "1.1.3";
        public static string _author = "山腳下的小黑熊";

        public static string _windowTitle = "RMTV Recorder";
        public static string _outputPath = "output";
        public static string _logPath = "log";

        public static string _ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\bin\\ffmpeg.exe");
        public static string _m3u8_es_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\rmtv-es.m3u8");
        public static string _m3u8_en_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\rmtv-en.m3u8");
        public static string _keyfile_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\iamacrazylukamodriclover.txt");
        public static string _testfile_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\test.txt");

        public static string _timezoneIdSpain = "Romance Standard Time";
        public static string _timezoneIdTaiwan = "Taipei Standard Time";
        public static string _timezoneIdUTC = "UTC";

        public static string Language_Spanish = "Spanish";
        public static string Language_English = "English";

        public static int delay_sec = 1;
    }

    class Global
    {
        public static List<RecObj> _groupRecObj;
        public static System.Windows.Forms.NotifyIcon _notifyIcon;
        public static bool flagTaskComplete = false;
    }

    public delegate void OperationHandler();

}
