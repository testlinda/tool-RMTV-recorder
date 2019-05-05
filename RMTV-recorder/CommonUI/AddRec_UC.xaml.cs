﻿
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
            InitailEndTime();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            base.OnCloseDialog(this, false, e);
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (CheckData())
            {
                //Debug.WriteLine(GetSpainTime());
                //Debug.WriteLine(GetStartTime());
                //Debug.WriteLine(GetEndTime());
                //Debug.WriteLine(GetDuration());

                RecObj recObj = new RecObj
                {
                    Index = GetIndex(),
                    Language = GetLanguage(),
                    StartTime = GetStartTime(),
                    EndTime = GetEndTime(),
                    Duration = GetDuration(),
                    Status = RecObj.RecordStatus.Scheduled,
                    Log = "",
                    RetryTimes = 0,
                };

                recObj.Initialization();
                CommonFunc.AddRecObj(recObj);

                base.OnCloseDialog(this, true, e);
            }
        }

        private bool CheckData()
        {
            if (!IsStartTimeDataAcceptable() ||
                !IsEndTimeDataAcceptable() ||
                !IsDurationDataAcceptable())
                return false;

            return true;
        }

        private bool IsStartTimeDataAcceptable()
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
            }

            return true;
        }

        private bool IsEndTimeDataAcceptable()
        {
            if (rb_set_endtime.IsChecked == true)
            {
                if (tb_endtime_hour.Text.Equals("") ||
                    tb_endtime_min.Text.Equals(""))
                {
                    MessageBox.Show("End time is invalid.", "Error");
                    return false;
                }
                else
                {
                    if (int.Parse(tb_endtime_hour.Text) > 23 ||
                        int.Parse(tb_endtime_min.Text) > 59)
                    {
                        MessageBox.Show("End time is invalid.", "Error");
                        return false;
                    }

                    if (GetStartTime().Day > CommonFunc.GetSpainTime().Day)
                    {
                        if (int.Parse(tb_endtime_hour.Text) * 100 + int.Parse(tb_endtime_min.Text) <=
                            int.Parse(tb_statrttime_hour.Text) * 100 + int.Parse(tb_statrttime_hour.Text))
                        {
                            MessageBox.Show("End time is set eailer than start time.", "Error");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool IsDurationDataAcceptable()
        {
            if (rb_set_endtime.IsChecked == false)
            {
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
            }

            return true;
        }

        private bool IsPreviousTime(DateTime time1, DateTime time2) //true, if time2 >= time1
        {
            if (DateTime.Compare(time1, time2) <= 0)
                return true;

            return false;
        }        

        private int CalculateDuration2min()
        {
            int duration = 0;
            if (rb_set_endtime.IsChecked == true)
            {
                TimeSpan ts = GetEndTime() - GetStartTime();
                double differenceInMinutes = ts.TotalMinutes;
                duration = (int)differenceInMinutes;
            }
            else
            {
                duration = int.Parse(tb_duration_hour.Text) * 60 + int.Parse(tb_duration_min.Text);
            }

            return duration;
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
                return Parameter.Language_Spanish ;
            else
                return Parameter.Language_English;
        }

        private DateTime GetStartTime()
        {
            DateTime date;

            if (rb_starttime_now.IsChecked == true)
            {
                date = CommonFunc.GetSpainTime().AddSeconds(Parameter.delay_sec);
            }
            else
            {
                date = CommonFunc.SetSpainTime(int.Parse(tb_statrttime_hour.Text),
                                    int.Parse(tb_statrttime_min.Text));

                if (IsPreviousTime(date, CommonFunc.GetSpainTime()))
                {
                    date = date.AddDays(1);
                }
            }

            return date;
        }

        private DateTime GetEndTime()
        {
            DateTime date;

            if (rb_set_endtime.IsChecked == true)
            {
                date = CommonFunc.SetSpainTime(int.Parse(tb_endtime_hour.Text),
                                    int.Parse(tb_endtime_min.Text));

                if (IsPreviousTime(date, GetStartTime()))
                {
                    date = date.AddDays(1);
                }
            }
            else
            {
                date = GetStartTime().AddSeconds(CalculateDuration2sec()); ;
            }

            return date;
        }

        private int GetDuration()
        {
            return CalculateDuration2min();
        }

        private void InitailEndTime()
        {
            tb_endtime_hour.Text = CommonFunc.GetSpainTime().AddHours(1).Hour.ToString("00");
            tb_endtime_min.Text = CommonFunc.GetSpainTime().Minute.ToString("00");
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

        private void RadioButton_StartTime_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (tb_statrttime_hour == null || tb_statrttime_min == null)
                return;

            tb_statrttime_hour.IsEnabled = !(rb_starttime_now.IsChecked == true);
            tb_statrttime_min.IsEnabled = !(rb_starttime_now.IsChecked == true);
            tb_statrttime_hour.Text = !(rb_starttime_now.IsChecked == true)? "11":"";
            tb_statrttime_min.Text = !(rb_starttime_now.IsChecked == true) ? "00" : "";
        }

        private void RadioButton_EndTime_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (tb_endtime_hour == null || tb_endtime_min == null ||
                tb_duration_hour == null || tb_duration_min == null)
                return;

            if (rb_set_endtime.IsChecked == true)
            {
                InitailEndTime();
            }
            else
            {
                tb_endtime_hour.Text = "";
                tb_endtime_min.Text = "";
            }
            
            tb_endtime_hour.IsEnabled = (rb_set_endtime.IsChecked == true);
            tb_endtime_min.IsEnabled = (rb_set_endtime.IsChecked == true);
           
            tb_duration_hour.IsEnabled = !(rb_set_endtime.IsChecked == true);
            tb_duration_min.IsEnabled = !(rb_set_endtime.IsChecked == true);
            tb_duration_hour.Text = !(rb_set_endtime.IsChecked == true) ? "1" : "";
            tb_duration_min.Text = !(rb_set_endtime.IsChecked == true) ? "0" : "";
        }
    }
}
