﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RMTV_recorder
{
    class CommonFunc
    {
        public static bool GetStatusChangedFlag()
        {
            return Global.flagTaskComplete;
        }

        public static void RaiseStatusChangedFlag()
        {
            Global.flagTaskComplete = true;
        }

        public static void ClearStatusChangedFlag()
        {
            Global.flagTaskComplete = false;
        }

        public static void AddRecObj(RecObj recObj)
        {
            lock(Global._syncLock)
            {
                Global._groupRecObj.Add(recObj);
            }
        }

        public static void RemoveRecObj(RecObj recObj)
        {
            lock (Global._syncLock)
            {
                Global._groupRecObj.Remove(recObj);
            }
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

        public static DateTime ConvertDateTime2Local(DateTime datetime_spain)
        {
            TimeZoneInfo timeZoneInfo_spain = TimeZoneInfo.FindSystemTimeZoneById(Parameter._timezoneIdSpain);
            DateTime datetime_Unspec = DateTime.SpecifyKind(datetime_spain, DateTimeKind.Unspecified);
            DateTime datetime_Utc = TimeZoneInfo.ConvertTimeToUtc(datetime_Unspec, timeZoneInfo_spain);
            DateTime datetime_local = datetime_Utc.ToLocalTime();

            return datetime_local;
        }

        public static DateTime ConvertDateTime2Spain(DateTime datetime_local)
        {
            datetime_local = DateTime.SpecifyKind(datetime_local, DateTimeKind.Local);
            DateTime datetime_spain = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(datetime_local.ToUniversalTime(), Parameter._timezoneIdSpain);

            return datetime_spain;
        }

        public static DateTime GetSpainTime()
        {
            return ConvertDateTime2Spain(DateTime.Now);
        }

        public static DateTime SetSpainTime(int hour, int minute)
        {
            DateTime dateNow = GetSpainTime();
            DateTime dateReset = dateNow.AddHours(dateNow.Hour * (-1))
                                               .AddMinutes(dateNow.Minute * (-1))
                                               .AddSeconds(dateNow.Second * (-1))
                                               .AddMilliseconds(dateNow.Millisecond * (-1));

            DateTime date = dateReset.AddHours(hour).AddMinutes(minute);
            return date;
        }

        public static void UpdateM3U8()
        {
            DownloadFile d = new DownloadFile();
            d.DownloadESM3U8();
            d.DownloadENM3U8();
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

        public static void test()
        {
            //MessageBox.Show("Test!");
            Thread.Sleep(5000);
            Debug.WriteLine("Hello");
            Debug.WriteLine("Hello");
            Debug.WriteLine("Hello");
            Debug.WriteLine("Hello");
            Debug.WriteLine("Hello");
        }
    }
}
