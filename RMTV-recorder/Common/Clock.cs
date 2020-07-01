using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Globalization;

namespace RMTV_recorder
{
    class Clock
    {
        private Timer _timer = null;
        private Window _window = null;
        private Label _label_date = null;
        private Label _label_time = null;
        private string _time_zone = Parameter._timezoneIdUTC;

        public Clock(Window window, Label label_date, Label label_time, string time_zone)
        {
            _window = window;
            _label_date = label_date;
            _label_time = label_time;
            _time_zone = time_zone;

            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnClockEvent);
            _timer.Interval = 1000;

            if (_window == null || _label_time == null)
            {
                //Error
            }
        }

        public void StartClock()
        {
            if (_timer == null)
                return;

            _timer.Enabled = true;
        }

        public void PauseClock()
        {
            if (_timer == null)
                return;

            _timer.Enabled = false;
        }

        public void TerminateClock()
        {
            _timer.Enabled = false;
            _timer.Dispose();
        }

        public void UpdateTimeZone(string time_zone)
        {
            PauseClock();
            _label_time.Content = "--";
            _label_date.Content = "--";
            _time_zone = time_zone;
            StartClock();
        }

        private void OnClockEvent(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.Dispatcher.Invoke(DispatcherPriority.Normal, new OperationHandler(updateClock));
            }
        }

        private void updateClock()
        {
            DateTime time = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _time_zone);
            _label_date.Content = time.ToString("yyyy-MM-dd ddd", CultureInfo.CreateSpecificCulture("en-US"));
            _label_time.Content = time.ToString("HH:mm:ss");
        }

    }

    class ClockTimer
    {
        private Timer timer = null;
        private Window _window = null;
        private Label _label = null;
        private int timerCount = 0;

        public ClockTimer(Window window, Label label)
        {
            _window = window;
            _label = label;
        }

        public int StartTimer()
        {
            if (_window == null || _label == null)
            {
                return -1;
            }
            else
            {
                timer = new Timer();
                timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
                timer.Interval = 1000;
                timer.Enabled = true;
                return 0;
            }
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.Dispatcher.Invoke(DispatcherPriority.Normal, new OperationHandler(updateTimer));
            }
        }

        private void updateTimer()
        {
            timerCount++;
            TimeSpan tSpan = TimeSpan.FromSeconds(timerCount);
            _label.Content = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                    tSpan.Hours,
                                    tSpan.Minutes,
                                    tSpan.Seconds);
        }

        public void StopTimer()
        {
            timerCount = 0;
            if (timer != null)
            {
                timer.Enabled = false;
            }

        }
    }

}
