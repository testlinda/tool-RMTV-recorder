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
        private Timer timer = null;
        private delegate void TimerDispatcherDelegate();
        private Window _window = null;
        private Label _label = null;

        private bool _isLocal = false;
        private string _time_zone = Parameter._timezoneIdUTC;
        private int timerCount = 0;

        public Clock(Window window, Label label)
        {
            _window = window;
            _label = label;
        }

        public Clock(Window window, Label label, bool isLocal)
        {
            _window = window;
            _label = label;
            _isLocal = isLocal;
        }

        public Clock(Window window, Label label, string time_zone)
        {
            _window = window;
            _label = label;
            _time_zone = time_zone;
        }

        public int StartClock()
        {
            if (_window == null || _label == null)
            {
                return -1;
            }
            else
            {
                timer = new Timer(1000);
                timer.Elapsed += new ElapsedEventHandler(OnClockEvent);
                timer.Interval = 1000;
                timer.Enabled = true;
                return 0;
            }
        }

        private void OnClockEvent(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.Dispatcher.Invoke(DispatcherPriority.Normal, new TimerDispatcherDelegate(updateClock));
            }
        }

        private void updateClock()
        {
            if (_isLocal)
                _label.Content = DateTime.Now.ToString("HH:mm:ss");
            else
            {
                var datetime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _time_zone);
                _label.Content = datetime.ToString("HH:mm:ss");
            }
        }

        public int StartTimer()
        {
            if (_window == null || _label == null)
            {
                return -1;
            }
            else
            {
                timer = new Timer(1000);
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
                _window.Dispatcher.Invoke(DispatcherPriority.Normal, new TimerDispatcherDelegate(updateTimer));
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
