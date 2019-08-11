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
        public static readonly string _uniqueStrPrefix = "rmtv_recorder_";
        public static readonly string _resourcePath = "Resource";
        public static readonly string _outputPath = "output";
        public static readonly string _logPath = "log";
        public static readonly string _keyString = "aWFtYWNyYXp5bHVrYW1vZHJpY2xvdmVyLnR4dA==";

        public static readonly string _resourceFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource");
        public static readonly string _ffmpegPath = Path.Combine(_resourceFullPath, "bin\\ffmpeg.exe");
        public static readonly string _setting_Path= Path.Combine(_resourceFullPath, "setting.ini");
        public static readonly string _m3u8_es_Path = Path.Combine(_resourceFullPath, "rmtv-es.m3u8");
        public static readonly string _m3u8_en_Path = Path.Combine(_resourceFullPath, "rmtv-en.m3u8");

        public static readonly string _timezoneIdSpain = "Romance Standard Time";
        public static readonly string _timezoneIdTaiwan = "Taipei Standard Time";
        public static readonly string _timezoneIdUTC = "UTC";

        public static readonly string Channel_Spanish = "Spanish";
        public static readonly string Channel_English = "English";
        public static readonly string Channel_Custom = "Custom";

        public static readonly string uri_RMTV_es = @"https://www.realmadrid.com/real-madrid-tv";
        public static readonly string uri_RMTV_m3u8_es = @"https://rmtv24hweblive-lh.akamaihd.net/i/rmtv24hwebes_1@300661/master.m3u8";
        public static readonly string uri_RMTV_m3u8_en = @"https://rmtv24hweblive-lh.akamaihd.net/i/rmtv24hweben_1@300662/master.m3u8";

        //ini setting
        public readonly static string _iniSectionSetting = "Setting";
        public readonly static string _iniKeyTimeZoneId = "TimeZoneId";
        public readonly static string _iniKeyDebugMode = "DebugMode";

        //other
        public static readonly int delay_sec = 1;
        public static readonly int retry_times_limit = 10;
        public static readonly int retry_wait_min_sec = 3;
        public static readonly int retry_wait_max_sec = 60;
        public static readonly int disconnection_diff_min = 3;
        public static readonly int debug_on_click = 3;

    }

    class GlobalVar
    {
        public static RecObjCollection _RecObjs;
        public static System.Windows.Forms.NotifyIcon _notifyIcon;
        public static string _uniqueStr = Parameter._uniqueStrPrefix;
        public static string _timezoneId = Parameter._timezoneIdUTC;
        public static bool flagTaskComplete = false;
        public static List<M3U8Obj> _rmtv_link_es = null;
        public static List<M3U8Obj> _rmtv_link_en = null;

        public static bool _debugmode = false;
        
        
    }

    public delegate void OperationHandler();
    public delegate bool OperationHandlerWithResult();

}
