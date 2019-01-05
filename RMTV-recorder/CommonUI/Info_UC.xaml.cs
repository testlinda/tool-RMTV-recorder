using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for Info_UC.xaml
    /// </summary>
    public partial class Info_UC : ucCustom
    {
        public Info_UC()
        {
            InitializeComponent();
            label_version.Content = Parameter._version;
            label_author.Content = Parameter._author;
        }
    }
}
