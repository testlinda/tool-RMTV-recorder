using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RMTV_recorder
{
    /// <summary>
    /// Interaction logic for Processing_UC.xaml
    /// </summary>
    public partial class Processing_UC : ucCustom
    {
        private BackgroundWorker backgroundWorker = null;
        private Window _windows = null;
        private OperationHandler operation_delegate = null;
        private OperationHandlerWithResult operation_delegate_with_result = null;
        private bool result;

        public Processing_UC()
        {
            InitializeComponent();
            this.Loaded += Processing_UC_Load;
        }

        public string Message { get; set; }
        public bool ShowProgressBar { get; set; }
        public string ButtonText { get; set; }
        public Window window
        {
            set
            {
                _windows = value;
            }
        }

        public OperationHandler Operation_delegate
        {
            set
            {
                operation_delegate = value;
            }
        }

        public OperationHandlerWithResult Operation_delegate_with_result
        {
            set
            {
                operation_delegate_with_result = value;
            }
        }

        private void Initialization()
        {
            //if (_windows != null)
            //{
            //    _windows.WindowStyle = WindowStyle.None;
            //    _windows.AllowsTransparency = true;
            //}

            tb_message.Text = Message;
            progressbar.Visibility = (ShowProgressBar) ? Visibility.Visible : Visibility.Collapsed;
            button.Content = ButtonText;
        }

        private void Processing_UC_Load(object sender, RoutedEventArgs e)
        {
            Initialization();

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            this.backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (operation_delegate != null)
            {
                operation_delegate();
                return;
            }

            if (operation_delegate_with_result != null)
            {
                result = operation_delegate_with_result();
            }
                
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (operation_delegate != null)
            {
                base.OnCloseDialog(this, false, e);
            }

            if (operation_delegate_with_result != null)
            {
                base.OnCloseDialog(this, result, e);
            }
            
        }

        delegate void UpdateMessageCallback(string text);
        public void UpdateMessage(string message)
        {
            if (tb_message.Dispatcher.CheckAccess())
            {
                tb_message.Text = message;
            }
            else
            {
                UpdateMessageCallback d = new UpdateMessageCallback(UpdateMessage);
                tb_message.Dispatcher.Invoke(d, new object[] { message });
            }
        }
    }
}
