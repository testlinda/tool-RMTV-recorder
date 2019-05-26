using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls;

namespace RMTV_recorder
{
    class Clock
    {
        private Timer _timer = null;
        private Window _window = null;
        private Label _label = null;
        private string _time_zone = Parameter._timezoneIdUTC;

        public Clock(Window window, Label label, string time_zone)
        {
            _window = window;
            _label = label;
            _time_zone = time_zone;

            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnClockEvent);
            _timer.Interval = 1000;

            if (_window == null || _label == null)
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
            _label.Content = "--";
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
            _label.Content = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _time_zone).ToString("HH:mm:ss");
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
