using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

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
