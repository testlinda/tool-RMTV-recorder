using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RMTV_recorder
{
    class CommonFunc
    {
        private static Window _window = null;

        public Window window
        {
            set
            {
                _window = value;
            }
        }

        public static bool GetStatusChangedFlag()
        {
            return GlobalVar.flagTaskComplete;
        }

        public static void RaiseStatusChangedFlag()
        {
            GlobalVar.flagTaskComplete = true;
        }

        public static void ClearStatusChangedFlag()
        {
            GlobalVar.flagTaskComplete = false;
        }

        public static bool PrepareShutDown(Window window)
        {
            winCustom wincustom = new winCustom();
            ShutDownDialog_UC shutdownDialog_uc = new ShutDownDialog_UC();
            OperationHandler shutdown = new OperationHandler(ShutDown);

            shutdownDialog_uc.window = window;
            shutdownDialog_uc.Operation_delegate = shutdown;
            wincustom.winContent = shutdownDialog_uc;
            wincustom.Title = "Warning";
            wincustom.CloseButtonVisible = false;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            wincustom.Topmost = true;
            wincustom.ShowInTaskbar = false;
            return (wincustom.ShowDialog() == true);
        }

        private static void ShutDown()
        {
#if DEBUG
            MessageBox.Show("Time is up!");
            return;
#endif

            var psi = new ProcessStartInfo("shutdown", "/s /t 10");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        public static void IntialUniqleString()
        {
#if DEBUG
            GlobalVar._uniqueStr += "debugtest";
#else
            GlobalVar._uniqueStr += GetKeyFileStr();
#endif
        }

        public static string GetKeyFileStr()
        {
            string strKey = "";
            string keyFile = GetKeyfilePath();
            if (File.Exists(keyFile))
            {
                using (StreamReader reader = new StreamReader(keyFile))
                {
                    strKey = reader.ReadLine() ?? "";
                }
            }

            return strKey;
        }

        public static DateTime ConvertDateTime2Local(string timezoneId, DateTime datetime_zone)
        {
            TimeZoneInfo timeZoneInfo_zone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            DateTime datetime_Unspec = DateTime.SpecifyKind(datetime_zone, DateTimeKind.Unspecified);
            DateTime datetime_Utc = TimeZoneInfo.ConvertTimeToUtc(datetime_Unspec, timeZoneInfo_zone);
            DateTime datetime_local = datetime_Utc.ToLocalTime();

            return datetime_local;
        }

        public static DateTime ConvertDateTime2Zone(string timezoneId, DateTime datetime_local)
        {
            datetime_local = DateTime.SpecifyKind(datetime_local, DateTimeKind.Local);
            DateTime datetime_zone = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(datetime_local.ToUniversalTime(), timezoneId);

            return datetime_zone;
        }

        public static DateTime GetZoneTime(string timezoneId)
        {
            return ConvertDateTime2Zone(timezoneId, DateTime.Now);
        }

        public static DateTime SetZoneTime(string timezoneId, int hour, int minute)
        {
            DateTime dateNow = GetZoneTime(timezoneId);
            DateTime dateReset = dateNow.AddHours(dateNow.Hour * (-1))
                                        .AddMinutes(dateNow.Minute * (-1))
                                        .AddSeconds(dateNow.Second * (-1))
                                        .AddMilliseconds(dateNow.Millisecond * (-1));

            DateTime dateSet = dateReset.AddHours(hour).AddMinutes(minute);
            return dateSet;
        }

        public static DateTime SetZoneTime(string timezoneId, DateTime date, int hour, int minute)
        {
            DateTime dateSet = date.AddHours(hour)
                                   .AddMinutes(minute);

            return dateSet;
        }

        public static string GetTimeZoneHour(string timezoneId)
        {
            double timeZoneHour = 0;
            try
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                timeZoneHour = info.BaseUtcOffset.TotalHours;
            }
            catch
            {

            }

            return timeZoneHour < 0 ? timeZoneHour.ToString() : "+" + timeZoneHour.ToString();
        }

        public static string GetTimeZoneDisplayName(string timezoneId)
        {
            string timeZoneName = "";
            try
            {
                timeZoneName = TimeZoneInfo.FindSystemTimeZoneById(GlobalVar._timezoneId).DisplayName;
            }
            catch
            {

            }

            return timeZoneName;
        }

        public static void RunWithProcessingUC(OperationHandler operation, string message)
        {
            winCustom wincustom = new winCustom();
            Processing_UC processing_uc = new Processing_UC();

            processing_uc.window = wincustom;
            processing_uc.Operation_delegate = operation;
            processing_uc.Message = message;
            processing_uc.ShowProgressBar = true;
            wincustom.winContent = processing_uc;
            wincustom.Topmost = true;
            wincustom.Owner = _window;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.WindowStyle = WindowStyle.None;
            wincustom.AllowsTransparency = true;
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();
        }

        public static bool RunWithProcessingUC(OperationHandlerWithResult operation, string message)
        {
            winCustom wincustom = new winCustom();
            Processing_UC processing_uc = new Processing_UC();

            processing_uc.window = wincustom;
            processing_uc.Operation_delegate_with_result = operation;
            processing_uc.Message = message;
            processing_uc.ShowProgressBar = true;
            wincustom.winContent = processing_uc;
            wincustom.Topmost = true;
            wincustom.Owner = _window;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.WindowStyle = WindowStyle.None;
            wincustom.AllowsTransparency = true;
            wincustom.ShowInTaskbar = false;
            return (wincustom.ShowDialog() == true);
        }

        public static void InitialM3U8()
        {
            M3U8 m = new M3U8();
            m.InitialM3U8List();
        }

        public static bool DownloadM3U8()
        {
            DownloadFile d = new DownloadFile();
            return d.DownloadESM3U8() & d.DownloadENM3U8();
        }

        public static TimeSpan GetVideoDuration(string filepath)
        {
            Process process = new Process();
            System.Text.RegularExpressions.Regex regex = null;
            System.Text.RegularExpressions.Match match = null;
            System.IO.StreamReader srouput = null;
            string output = "";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Parameter._ffmpegPath;
            startInfo.Arguments = string.Format("-i \"{0}\"", filepath);
            startInfo.RedirectStandardError = true;
            process = Process.Start(startInfo);

            srouput = process.StandardError;
            output = srouput.ReadToEnd();

            process.WaitForExit();
            process.Close();
            process.Dispose();
            srouput.Close();
            srouput.Dispose();

            regex = new System.Text.RegularExpressions.Regex("[D|d]uration:.((\\d|:|\\.)*)");
            match = regex.Match(output);

            if (match.Success)
            {
                string[] matchpieces = match.Value.Split(new char[] {' '});
                string[] timepieces = matchpieces[1].Split(new char[] { ':', '.' });
                if (timepieces.Length == 4)
                {
                    return new TimeSpan(0, Convert.ToInt16(timepieces[0]), Convert.ToInt16(timepieces[1]), Convert.ToInt16(timepieces[2]), Convert.ToInt16(timepieces[3]));
                }
            }

            return TimeSpan.Zero;
        }

        public static bool IsUrlValid(string url)
        {
            bool pageExists = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Head;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                pageExists = response.StatusCode == HttpStatusCode.OK;
                response.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return pageExists;
        }

        public static bool IsFileValid(string url)
        {
            bool ret = false;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                WebHeaderCollection headers = webRequest.GetResponse().Headers;
                ret = true;
            }
            catch (WebException ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        public static bool CheckChannelLinkValid(string channel, string channellink)
        {
            if (channel.Equals(Parameter.Channel_Spanish))
            {
                foreach (M3U8Obj obj in GlobalVar._rmtv_link_es)
                {
                    if (IsFileValid(obj.Path))
                    {
                        return true;
                    }
                }
            }
            else if (channel.Equals(Parameter.Channel_English))
            {
                channellink = Parameter._m3u8_en_Path;
                foreach (M3U8Obj obj in GlobalVar._rmtv_link_en)
                {
                    if (IsFileValid(obj.Path))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (IsFileValid(channellink))
                {
                    return true;
                }
            }
        
            return false;
        }

        public static void ToastMessage(Label label, string message, int msg_duration)
        {
            label.Content = message;

            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, msg_duration);
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = (double)msg_duration;
            animation.To = 0.0;
            animation.Duration = new Duration(duration);
            Storyboard.SetTargetName(animation, label.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
            storyboard.Children.Add(animation);

            storyboard.Begin(label);
        }

        public static string GetKeyfilePath()
        {
            return Path.Combine(Parameter._resourceFullPath, Base64Decode(Parameter._keyString));
        }

        public static string Base64Encode(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(data);
        }

        public static string Base64Decode(string str)
        {
            byte[] data = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(data);
        }

        public static void test()
        {
            //MessageBox.Show("Test!");
            //Thread.Sleep(5000);
            //Debug.WriteLine("Hello");
            //Debug.WriteLine("Hello");
            //Debug.WriteLine("Hello");
            //Debug.WriteLine("Hello");
            //Debug.WriteLine("Hello");
        }
    }
}
