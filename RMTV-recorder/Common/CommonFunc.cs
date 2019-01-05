using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace RMTV_recorder
{
    class CommonFunc
    {
        public static void PrepareShutDown(Window window)
        {
            winCustom wincustom = new winCustom();
            ShutDownDialog_UC shutdownDialog_uc = new ShutDownDialog_UC();
            OperationHandler shutdown = new OperationHandler(ShutDown);

            shutdownDialog_uc.window = window;
            shutdownDialog_uc.Operation_delegate = shutdown;
            wincustom.winContent = shutdownDialog_uc;
            wincustom.Title = "Warning";
            wincustom.CloseButtonVisible = false;
            wincustom.Topmost = true;
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();
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
            TimeZoneInfo timeZoneInfo_local = TimeZoneInfo.Local;
            TimeZoneInfo timeZoneInfo_spain = TimeZoneInfo.FindSystemTimeZoneById(Parameter._timezoneIdSpain);

            return TimeZoneInfo.ConvertTime(datetime_spain, timeZoneInfo_spain, timeZoneInfo_local);
        }

        public static DateTime ConvertDateTime2Spain(DateTime datetime_local)
        {
            TimeZoneInfo timeZoneInfo_local = TimeZoneInfo.Local;
            TimeZoneInfo timeZoneInfo_spain = TimeZoneInfo.FindSystemTimeZoneById(Parameter._timezoneIdSpain);

            return TimeZoneInfo.ConvertTime(datetime_local, timeZoneInfo_local, timeZoneInfo_spain);
        }
    }
}
