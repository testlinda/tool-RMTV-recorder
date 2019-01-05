using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RMTV_recorder
{
    /// <summary>
    /// Interaction logic for ShutDownDialog_UC.xaml
    /// </summary>
    public partial class ShutDownDialog_UC : ucCustom
    {
        private int CountdownTime = 10;
        private Window _window = null;
        Timer _timer = null;
        Timer _timer_countdown = null;
        private delegate void TimerDispatcherDelegate();
        private int intCountDown = 0;

        private OperationHandler operation_delegate = null;

        public Window window
        {
            set
            {
                _window = value;
            }
        }

        public OperationHandler Operation_delegate
        {
            set
            {
                operation_delegate = value;
            }
        }

        public ShutDownDialog_UC()
        {
            InitializeComponent();
            this.Loaded += ShutDownDialog_UC_Load;
        }

        private void ShutDownDialog_UC_Load(object sender, RoutedEventArgs e)
        {
            intCountDown = CountdownTime;
            label_sec.Content = intCountDown;
            this.CloseDialog += new CloseDialogHandler(CloseShutDownDialog);

            DisplayCountDownTime();
            CountTime();
        }

        private void CountTime()
        {
            _timer = new Timer();
            _timer.Interval = CountdownTime * 1000;
            _timer.Elapsed += new ElapsedEventHandler(time_Elapsed);
            _timer.Enabled = true;
        }

        private void time_Elapsed(object sender, ElapsedEventArgs e)
        {
            //When time's up, do sth here
            _timer.Enabled = false;
            
            if (operation_delegate != null)
                operation_delegate();
        }

        private void DisplayCountDownTime()
        {
            _timer_countdown = new Timer(1000);
            _timer_countdown.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            _timer_countdown.Interval = 1000;
            _timer_countdown.Enabled = true;
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            if (_window != null)
            {
                _window.Dispatcher.Invoke(DispatcherPriority.Normal, new TimerDispatcherDelegate(updateCountDown));
            }
        }

        private void updateCountDown()
        {
            intCountDown--;

            if (intCountDown < 0)
            {
                _timer_countdown.Enabled = false;
                return;
            }
            
            label_sec.Content = intCountDown;
        }

        void CloseShutDownDialog(object sender, bool bApply, EventArgs e)
        {
            _timer.Enabled = false;
            _timer_countdown.Enabled = false;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            base.OnCloseDialog(this, false, e);
        }
    }
}
