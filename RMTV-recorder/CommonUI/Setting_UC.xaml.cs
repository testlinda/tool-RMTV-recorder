using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Setting_UC.xaml
    /// </summary>
    public partial class Setting_UC : ucCustom
    {
        public Setting_UC()
        {
            InitializeComponent();
            Initializaton();
        }

        private void Initializaton()
        {
            ReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();
            cb_timezone.DataContext = TimeZones;
            cb_timezone.SelectedItem = TimeZoneInfo.FindSystemTimeZoneById(GlobalVar._timezoneId);
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            TimeZoneInfo info = (TimeZoneInfo)cb_timezone.SelectedItem;
            GlobalVar._timezoneId = info.Id;

            base.OnCloseDialog(this, true, e);
        }


        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            base.OnCloseDialog(this, false, e);
        }
    }
}
