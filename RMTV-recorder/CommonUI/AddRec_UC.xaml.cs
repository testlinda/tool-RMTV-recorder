using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace RMTV_recorder
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AddRec_UC : ucCustom
    {
        public AddRec_UC()
        {
            InitializeComponent();
            Initializaton();
        }

        private void Initializaton()
        {

        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            base.OnCloseDialog(this, false, e);
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (CheckData())
            {
                RecObj recObj = new RecObj
                {
                    Index = GetIndex(),
                    Language = GetLanguage(),
                    StartTime = GetStartTime(),
                    StartTimeIsNow = (rb_starttime_now.IsChecked == true),
                    EndTime = GetEndTime(),
                    Duration = GetDuration(),
                    Status = RecObj.RecordStatus.WillBeRecorded,
                    Log = ""
                };

                //Debug.WriteLine(GetSpainTime());
                //Debug.WriteLine(GetStartTime());
                //Debug.WriteLine(GetEndTime());
                //Debug.WriteLine(GetDuration());

                Global._groupRecObj.Add(recObj);
                base.OnCloseDialog(this, true, e);
            }
        }

        

        private bool CheckData()
        {
            if (rb_starttime_now.IsChecked == false)
            {
                if (tb_statrttime_hour.Text.Equals("") ||
                    tb_statrttime_min.Text.Equals(""))
                {
                    MessageBox.Show("Start time is invalid.", "Error");
                    return false;
                }
                else
                {
                    if (int.Parse(tb_statrttime_hour.Text) > 23 ||
                        int.Parse(tb_statrttime_min.Text) > 59)
                    {
                        MessageBox.Show("Start time is invalid.", "Error");
                        return false;
                    }
                }

                if (IsPreviousTime())
                {
                    MessageBox.Show("The Start time is set at the past.", "Error");
                    return false;
                }
            }

            if (tb_duration_hour.Text.Equals("") ||
                tb_duration_min.Text.Equals(""))
            {
                MessageBox.Show("Duration is invalid.", "Error");
                return false;
            }

            if (CalculateDuration2min() < 1)
            {
                MessageBox.Show("Duration is too short.", "Error");
                return false;
            }

            return true;
        }

        private bool IsPreviousTime()
        {
            DateTime date = DateTime.Today
                .AddHours(int.Parse(tb_statrttime_hour.Text))
                .AddMinutes(int.Parse(tb_statrttime_min.Text));
            DateTime date_now = GetSpainTime(); //Time of Spain

            //Debug.WriteLine(string.Format("set {0:HH:mm:ss tt}", date));
            //Debug.WriteLine(string.Format("cuurent {0:HH:mm:ss tt}", date_now));

            if (DateTime.Compare(date, date_now) <= 0)
                return true;

            return false;
        }

        private DateTime GetSpainTime()
        {
            return CommonFunc.ConvertDateTime2Spain(DateTime.Now);
        }

        private int CalculateDuration2min()
        {
            if (tb_duration_hour.Text.Equals("") ||
                tb_duration_min.Text.Equals(""))
            {
                return 0;
            }

            int duration_hour = int.Parse(tb_duration_hour.Text);
            int duration_min = int.Parse(tb_duration_min.Text);

            return duration_hour * 60 + duration_min;
        }

        private int CalculateDuration2sec()
        {
            return CalculateDuration2min() * 60;
        }

        private int GetIndex()
        {
            return Global._groupRecObj.Count + 1;
        }

        private string GetLanguage()
        {
            if (rb_lang_spanish.IsChecked == true)
                return "Spanish";
            else
                return "English";
        }

        private DateTime GetStartTime()
        {
            DateTime date;

            if (rb_starttime_now.IsChecked == true)
            {
                date = GetSpainTime();
            }
            else
            {
                date = DateTime.Today
                .AddHours(int.Parse(tb_statrttime_hour.Text))
                .AddMinutes(int.Parse(tb_statrttime_min.Text));
            }

            return date;
        }

        private DateTime GetEndTime()
        {
            return GetStartTime().AddSeconds(CalculateDuration2sec());
        }

        private int GetDuration()
        {
            return CalculateDuration2min();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Int32 selectionStart = textBox.SelectionStart;
            Int32 selectionLength = textBox.SelectionLength;
            String newText = String.Empty;
            foreach (Char c in textBox.Text.ToCharArray())
            {
                if (Char.IsDigit(c) || Char.IsControl(c))
                {
                    newText += c;
                }
            }
            textBox.Text = newText;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
        }

        private void RadioButton_Time_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (tb_statrttime_hour == null || tb_statrttime_min == null)
                return;

            tb_statrttime_hour.IsEnabled = !(rb_starttime_now.IsChecked == true);
            tb_statrttime_min.IsEnabled = !(rb_starttime_now.IsChecked == true);
        }
    }
}
