using System;
using System.Collections.Generic;
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

namespace RMTV_recorder.CommonUI
{
    /// <summary>
    /// Interaction logic for Processing_UC.xaml
    /// </summary>
    public partial class Processing_UC : ucCustom
    {
        public Processing_UC()
        {
            InitializeComponent();
            Initialization();
        }

        public string Message { get; set; }
        public bool ShowProgressBar { get; set; }
        public string ButtonText { get; set; }

        private void Initialization()
        {
            tb_message.Text = Message;
            progressbar.Visibility = (ShowProgressBar) ? Visibility.Visible : Visibility.Collapsed;
            button.Content = ButtonText;
        }
    }
}
