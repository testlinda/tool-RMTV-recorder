
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RMTV_recorder
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AddRec_UC : ucCustom
    {
        private DateTime temp_currenttime = DateTime.MinValue;
        private DateTime temp_starttime = DateTime.MinValue;
        private DateTime temp_endtime = DateTime.MinValue;
        private BackgroundWorker linkValidator = null;
        private bool IsSelectDateDirty = false;

        public AddRec_UC()
        {
            InitializeComponent();
            Initializaton();
        }

        public string TimeZoneString
        {
            get
            {
                return "(UTC " + CommonFunc.GetTimeZoneHour(GlobalVar._timezoneId) + ")";
            }
        }

        public string TimeZoneToolTip
        {
            get
            {
                return CommonFunc.GetTimeZoneDisplayName(GlobalVar._timezoneId);
            }
        }

        private void Initializaton()
        {
            this.DataContext = this;

            InitailComboBox();
            InitailStartTimeDatePicker();
            InitailEndTime();
            InitailLinkValidator();

            if (GlobalVar._timezoneId.Equals(Parameter._timezoneIdSpain))
            {
                rb_channel_spanish.IsChecked = true;
            }
            else
            {
                rb_channel_custom.IsChecked = true;
            }

            grid_channel_custom.Visibility = Visibility.Collapsed;
            grid_startdate.Visibility = Visibility.Collapsed;
        }

        private void InitailEndTime()
        {
            tb_endtime_hour.Text = CommonFunc.GetZoneTime(GlobalVar._timezoneId).AddHours(1).Hour.ToString("00");
            tb_endtime_min.Text = CommonFunc.GetZoneTime(GlobalVar._timezoneId).Minute.ToString("00");
        }

        private void InitailComboBox()
        {
            cboxFilePath.ItemsSource = GlobalVar._commonUrlObjs.UrlObjs;
            cboxFilePath.Loaded += delegate
            {
                TextBox tb = (TextBox)cboxFilePath.Template.FindName("PART_EditableTextBox", cboxFilePath);
                if (tb != null)
                {
                    tb.LostFocus += new RoutedEventHandler(tboxFilePath_LostFocus);
                }
            };
        }

        private void InitailLinkValidator()
        {
            linkValidator = new BackgroundWorker();
            linkValidator.DoWork += linkValidator_DoWork;
            linkValidator.RunWorkerCompleted += linkValidator_WorkCompleted;
            linkValidator.WorkerSupportsCancellation = true;
        }

        private void InitailStartTimeDatePicker()
        {
            DateTime currentdate = CommonFunc.GetZoneTime(GlobalVar._timezoneId).Date;
            DateTime yesterdate = currentdate.AddDays(-1);
            datepicker_startdate.SelectedDate = currentdate;
            datepicker_startdate.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, new DateTime(yesterdate.Year, yesterdate.Month, yesterdate.Day)));
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            base.OnCloseDialog(this, false, e);
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            temp_currenttime = CommonFunc.GetZoneTime(GlobalVar._timezoneId);
            temp_starttime = GetStartTime();
            temp_endtime = GetEndTime();

            if (CheckData())
            {
                for (int i = 0; i< GetRepeatDays(); i++)
                {
                    RecObj recObj = new RecObj
                    {
                        Channel = GetChannel(),
                        ChannelLink = GetChannelLink(),
                        StartTime = temp_starttime.AddDays(i),
                        EndTime = temp_endtime,
                        Duration = GetDuration(),
                        TimeZoneId = GlobalVar._timezoneId,
                        Status = RecObj.RecordStatus.Scheduled,
                        Log = "",
                        RetryTimes = 0,
                    };

                    recObj.Initialization();
                    GlobalVar._RecObjs.Add(recObj);
                }

                base.OnCloseDialog(this, true, e);
            }
        }

        private void btnFileSelect_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Stream files (*.m3u8)|*.m3u8|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Parameter._resourceFullPath;

            if (openFileDialog.ShowDialog() == true)
                cboxFilePath.Text = openFileDialog.FileName;

            DisplayLinkAvailability(false, true, false);
        }

        private void Channel_RadioButton_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (rb_channel_custom == null)
                return;

            RadioButton rb = (RadioButton)sender;
            grid_channel_custom.Visibility = (rb.Name.Equals(rb_channel_custom.Name)) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool CheckData()
        {
            if (!IsStartTimeDataAcceptable() ||
                !IsEndTimeDataAcceptable() ||
                !IsDurationDataAcceptable() ||
                !IsCustomDataAcceptable() ||
                !IsRepeatDaysAcceptable())
                return false;

            return true;
        }

        private bool IsStartTimeDataAcceptable()
        {
            if (rb_starttime_now.IsChecked == false)
            {
                if (tb_starttime_hour.Text.Equals("") ||
                    tb_starttime_min.Text.Equals(""))
                {
                    MessageBox.Show("Start time is invalid.", "Error");
                    return false;
                }
                else
                {
                    if (Convert.ToInt32(tb_starttime_hour.Text) > 23 ||
                        Convert.ToInt32(tb_starttime_min.Text) > 59)
                    {
                        MessageBox.Show("Start time is invalid.", "Error");
                        return false;
                    }
                }

                if (temp_starttime < temp_currenttime)
                {
                    MessageBox.Show("Start time is set eailer than now.", "Error");
                    return false;
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
                    if (Convert.ToInt32(tb_endtime_hour.Text) > 23 ||
                        Convert.ToInt32(tb_endtime_min.Text) > 59)
                    {
                        MessageBox.Show("End time is invalid.", "Error");
                        return false;
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

        private bool IsCustomDataAcceptable()
        {
            if (rb_channel_custom.IsChecked == true && cboxFilePath.Text.Length == 0)
            {
                MessageBox.Show("Custom stream path is empty.", "Error");
                return false;
            }

            return true;
        }

        private bool IsChannelLinkAvailable()
        {
            string link = GetComboBoxText(cboxFilePath);
            bool IsFileExist = System.IO.File.Exists(link);
            bool IsUrlAccessible = CommonFunc.IsUrlValid(link);

            return IsFileExist || IsUrlAccessible;
        }

        private bool IsRepeatDaysAcceptable()
        {
            if (rb_repeat_none.IsChecked == true)
            {
                return true;
            }

            if (tb_repeat_times.Text.Equals(""))
            {
                MessageBox.Show("Repeat days is invalid.", "Error");
                return false;
            }
                

            int days = GetRepeatDays();
            if (days < 2)
            {
                MessageBox.Show("Repeat days can not be shorter than 2.", "Error");
                return false;
            }

            if (days > 10)
            {
                MessageBox.Show("Repeat days can not be more than 10.", "Error");
                return false;
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
                TimeSpan ts = temp_endtime - temp_starttime;
                double differenceInMinutes = ts.TotalMinutes;
                duration = (int)differenceInMinutes;
            }
            else
            {
                duration = Convert.ToInt32(tb_duration_hour.Text) * 60 + Convert.ToInt32(tb_duration_min.Text);
            }

            return duration;
        }

        private int CalculateDuration2sec()
        {
            return CalculateDuration2min() * 60;
        }

        private string GetChannel()
        {
            if (rb_channel_spanish.IsChecked == true)
                return Parameter.Channel_Spanish;
            else if (rb_channel_custom.IsChecked == true)
                return Parameter.Channel_Custom;
            else
                return Parameter.Channel_English;
        }

        private string GetChannelLink()
        {
            if (rb_channel_spanish.IsChecked == true)
                return Parameter.Channel_Spanish;
            else if (rb_channel_custom.IsChecked == true)
                return cboxFilePath.Text;
            else
                return Parameter.Channel_English;
        }

        private DateTime GetStartTime()
        {
            DateTime date;

            if (rb_starttime_now.IsChecked == true)
            {
                date = temp_currenttime.AddSeconds(Parameter.delay_sec);
            }
            else
            {
                date = CommonFunc.SetZoneTime(GlobalVar._timezoneId,
                                                  datepicker_startdate.SelectedDate.Value.Date,
                                                  Convert.ToInt32(tb_starttime_hour.Text),
                                                  Convert.ToInt32(tb_starttime_min.Text));
            }

            return date;
        }

        private DateTime GetEndTime()
        {
            DateTime date;

            if (rb_set_endtime.IsChecked == true)
            {
                date = CommonFunc.SetZoneTime(GlobalVar._timezoneId,
                                                  datepicker_startdate.SelectedDate.Value.Date,
                                                  Convert.ToInt32(tb_endtime_hour.Text),
                                                  Convert.ToInt32(tb_endtime_min.Text));

                if (IsPreviousTime(date, temp_starttime))
                {
                    date = date.AddDays(1);
                }

            }
            else
            {
                date = temp_starttime.AddSeconds(CalculateDuration2sec());
            }

            return date;
        }

        private int GetDuration()
        {
            return CalculateDuration2min();
        }

        private int GetRepeatDays()
        {
            if (rb_repeat_none.IsChecked == true)
                return 1;

            if (tb_repeat_times.Text.Equals(""))
                return 0;

            return Convert.ToInt32(tb_repeat_times.Text);
        }

        private void textBox_DigitOnly(object sender, TextChangedEventArgs e)
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
            if (tb_starttime_hour == null || tb_starttime_min == null)
                return;

            tb_starttime_hour.IsEnabled = !(rb_starttime_now.IsChecked == true);
            tb_starttime_min.IsEnabled = !(rb_starttime_now.IsChecked == true);
            tb_starttime_hour.Text = !(rb_starttime_now.IsChecked == true)? "11":"";
            tb_starttime_min.Text = !(rb_starttime_now.IsChecked == true) ? "00" : "";
            grid_startdate.Visibility = !(rb_starttime_now.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
            rb_repeat_times.Visibility = !(rb_starttime_now.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;

            if (rb_starttime_now.IsChecked == true)
            {
                rb_repeat_none.IsChecked = true;
            }
            else
            {
                AutoFillInDatePicker();
            }
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

        private void RadioButton_Repeat_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (rb_repeat_none == null || rb_repeat_times == null)
                return;

            tb_repeat_times.IsEnabled = !(rb_repeat_none.IsChecked == true);
            tb_repeat_times.Text = !(rb_repeat_none.IsChecked == true) ? "2" : "";
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void tboxFilePath_LostFocus(object sender, RoutedEventArgs e)
        {
            //TextBox tb = sender as TextBox;
            //Debug.WriteLine("-> Textbox LostFocus (IsFocused:{0})", tb.IsFocused);

            DisplayLinkAvailability(true, false, false);
            linkValidator.RunWorkerAsync();
        }

        private void linkValidator_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = IsChannelLinkAvailable();
        }

        private void linkValidator_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DisplayLinkAvailability(false, (bool)e.Result, !(bool)e.Result);
        }

        private void DisplayLinkAvailability(bool loading, bool ok, bool not_ok)
        {
            link_status.Visibility = (loading | ok | not_ok) ? Visibility.Visible : Visibility.Collapsed;
            img_status_loading.Visibility = (loading ) ? Visibility.Visible : Visibility.Collapsed;
            img_status_ok.Visibility = (ok) ? Visibility.Visible : Visibility.Collapsed;
            img_status_error.Visibility = (not_ok) ? Visibility.Visible : Visibility.Collapsed;
        }

        delegate string GetComboBoxTextCallback(ComboBox cbox);
        private string GetComboBoxText(ComboBox cbox)
        {
            if (cbox.Dispatcher.CheckAccess())
            {
                return cbox.Text;
            }
            else
            {
                GetComboBoxTextCallback d = new GetComboBoxTextCallback(GetComboBoxText);
                return (string)cbox.Dispatcher.Invoke(d, new object[] { cbox });
            }
        }

        private void TextBox_StartTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsSelectDateDirty) return;

            AutoFillInDatePicker();
        }

        private void AutoFillInDatePicker()
        {
            DateTime currenttime = CommonFunc.GetZoneTime(GlobalVar._timezoneId);
            DateTime date = CommonFunc.SetZoneTime(GlobalVar._timezoneId,
                                                      currenttime.Date,
                                                      Convert.ToInt32(tb_starttime_hour.Text),
                                                      Convert.ToInt32(tb_starttime_min.Text));

            if (IsPreviousTime(date, currenttime))
            {
                datepicker_startdate.SelectedDate = date.AddDays(1).Date;
            }
            else
            {
                datepicker_startdate.SelectedDate = date.Date;
            }
        }

        private void btnEditDate_Click(object sender, RoutedEventArgs e)
        {
            IsSelectDateDirty = true;

            datepicker_startdate.Style = null;
            btnEditDate.Visibility = Visibility.Collapsed;
        }
    }
}
