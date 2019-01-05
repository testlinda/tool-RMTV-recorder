using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    public partial class Log_UC : ucCustom
    {
        private string _log = string.Empty;
        private BackgroundWorker backgroundWorker = null;

        public Log_UC()
        {
            InitializeComponent();
            this.Loaded += Log_UC_Load;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        public string Log
        {
            set
            {
                _log = value;
            }
        }

        private void Log_UC_Load(object sender, RoutedEventArgs e)
        {
            btn_close.Visibility = Visibility.Hidden;
            this.backgroundWorker.RunWorkerAsync();

            tb_log.ScrollToEnd();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            base.OnCloseDialog(this, false, e);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SetText(_log);
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btn_close.Visibility = Visibility.Visible;
        }

        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            if (tb_log.Dispatcher.CheckAccess())
            {
                tb_log.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
            else
            {
                SetTextCallback d = new SetTextCallback(SetText);
                tb_log.Dispatcher.Invoke(d, new object[] { text });
            }
        }

    }
}
