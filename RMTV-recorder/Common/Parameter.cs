using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace RMTV_recorder
{
    class Parameter
    {
        public static readonly string _windowTitle = "RMTV Recorder";
        public static readonly string _outputPath = "output";
        public static readonly string _logPath = "log";

        public static readonly string _ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\bin\\ffmpeg.exe");
        public static readonly string _m3u8_es_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\rmtv-es.m3u8");
        public static readonly string _m3u8_en_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\rmtv-en.m3u8");
        public static readonly string _keyfile_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource\\iamacrazylukamodriclover.txt");

        public static readonly string _timezoneIdSpain = "Romance Standard Time";
        public static readonly string _timezoneIdTaiwan = "Taipei Standard Time";
        public static readonly string _timezoneIdUTC = "UTC";

        public static readonly string Language_Spanish = "Spanish";
        public static readonly string Language_English = "English";

        public static readonly string uri_RMTV_es = @"https://www.realmadrid.com/real-madrid-tv";

        public static readonly int delay_sec = 1;
        public static readonly int retry_times_limit = 10;
        public static readonly int retry_wait_min_sec = 3;
        public static readonly int retry_wait_max_sec = 15;
        public static readonly int disconnection_diff_min = 5;
        public static readonly int debug_on_click = 3;

        public static bool _debugmode = false;
    }

    class Global
    {
        public static ObservableCollection<RecObj> _groupRecObj;
        public static object _syncLock = new object();
        public static System.Windows.Forms.NotifyIcon _notifyIcon;
        public static bool flagTaskComplete = false;
    }

    public delegate void OperationHandler();

}
