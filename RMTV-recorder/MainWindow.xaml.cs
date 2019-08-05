using IniFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace RMTV_recorder
{
    public partial class MainWindow : Window
    {
        private bool isManuallyRecording = false;
        private bool load_success = false;

        private BackgroundWorker backgroundWorker_manual = null;
        private FFmpeg ffmpeg_manual = null;
        private Clock clock = null;
        private ClockTimer clocktimer_manual = null;
        private string _selectedChannel_manual = Parameter.Channel_Spanish ;
        private int timer_refreshdg_interval_sec = 60;
        private Timer timer_refreshdg = null;
        private Timer timer_manualrecord_checkalive = null;
        private int timer_manualrecord_checkalive_interval_sec = 2;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = Parameter._windowTitle;
            this.Loaded += Window_Loaded;

            load_success = Intialization();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!load_success || !LoadResource())
            {
                Application.Current.Shutdown();
            }
        }

        private bool Intialization()
        {

#if DEBUG
            AddTestItems();
#endif
            LoadIni();
            InitialCommonFunc();
            RefreshDebugMode();
            RunClock();
            InitialOutputFolder();
            InitialUI();
            InitialRecording();
            InitialNotifyIcon();

            Global._scheduledRecObj = new ScheduledRecObj();
            //Global._scheduledRecObj.RecObjs = new ObservableCollection<RecObj>();
            //Global._groupRecObj = new ObservableCollection<RecObj>();
            //BindingOperations.EnableCollectionSynchronization(Global._groupRecObj, Global._syncLock);
            dgRecObj.ItemsSource = Global._scheduledRecObj.RecObjs;
            Closing += OnClosing;

            return true;
        }

        private bool LoadResource()
        {
            if (!CheckKeyFileAvailable())
            {
                return false;
            }
            
            if (!CommonFunc.RunWithProcessingUC(
                            new OperationHandlerWithResult(CheckbinAvailable),
                            "Initialing..."))
            {
                return false;
            }

            return true;
        }

        private void LoadIni()
        {
            if (!File.Exists(Parameter._setting_Path))
            {
                CreateDefaultIniFile();
            }

            Global._timezoneId = IniHelper.ReadValue(Parameter._iniSectionSetting, Parameter._iniKeyTimeZoneId, Parameter._setting_Path);
            Global._debugmode = (IniHelper.ReadValue(Parameter._iniSectionSetting, Parameter._iniKeyDebugMode, Parameter._setting_Path).Equals("Y", StringComparison.OrdinalIgnoreCase));
        }

        private void InitialCommonFunc()
        {
            CommonFunc func = new CommonFunc();
            func.window = this;
        }

        private void CreateDefaultIniFile()
        {
            bool result = true;
            result = IniHelper.WriteValue(Parameter._iniSectionSetting, Parameter._iniKeyTimeZoneId, Parameter._timezoneIdSpain, Parameter._setting_Path);
            result = IniHelper.WriteValue(Parameter._iniSectionSetting, Parameter._iniKeyDebugMode, "N", Parameter._setting_Path);
        }

        private bool CheckKeyFileAvailable()
        {
            string keyFile = CommonFunc.GetKeyfilePath();
            if (!File.Exists(keyFile))
            {
                if (!CheckAuthentication())
                {
                    MessageBox.Show("Authentication failed!");
                    return false;
                }
            }

            return true;
        }

        private bool CheckbinAvailable()
        {
            if (!File.Exists(Parameter._ffmpegPath))
            {
                MessageBox.Show("ffmpeg.exe does not exist!");
                return false;
            }

            if (!File.Exists(Parameter._m3u8_es_Path))
            {
                DownloadFile d = new DownloadFile();
                if (!d.DownloadESM3U8())
                {
                    MessageBox.Show("Download RMTV file failed!");
                    return false;
                }
            }

            if (!File.Exists(Parameter._m3u8_en_Path))
            {
                DownloadFile d = new DownloadFile();
                if (!d.DownloadENM3U8())
                {
                    MessageBox.Show("Download RMTV file failed!");
                    return false;
                }
            }

            M3U8 m = new M3U8();
            if (!m.InitialM3U8List())
            {
                return false;
            }

            return true;
        }

        private bool CheckAuthentication()
        {
            winCustom wincustom = new winCustom();
            Authentication_UC authentication_uc = new Authentication_UC();

            wincustom.winContent = authentication_uc;
            wincustom.Title = "Authentication";
            wincustom.Topmost = true;
            wincustom.Owner = this;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.ShowInTaskbar = false;
            return (wincustom.ShowDialog() == true);
        }

        private void AddTestItems()
        {
            // A menu item in debug button named "Test" only showed in debug mode
            MenuItem menu_test = new MenuItem();
            menu_test.Header = "Test";
            menu_test.Click += btn_test_Click;

            menu_debug.Items.Add(menu_test);
        }

        private void RefreshDebugMode()
        {
            btn_debug.Visibility = (Global._debugmode) ? Visibility.Visible : Visibility.Collapsed;
            IniHelper.WriteValue(Parameter._iniSectionSetting, Parameter._iniKeyDebugMode, Global._debugmode ? "Y" : "N", Parameter._setting_Path);
        }

        private void RunClock()
        {
            UpdateClock();

            clock = new Clock(this, label_clock, Global._timezoneId);
            clock.StartClock();
        }

        private void InitialOutputFolder()
        {
            System.IO.Directory.CreateDirectory(Parameter._outputPath);
        }

        private void InitialUI()
        {
            InitialRecordButton();
            chechbox_isshutdown.Visibility = Visibility.Hidden;
        }

        private void InitialRecordButton()
        {
            clocktimer_manual = new ClockTimer(this, label_duration);
            SetRecordButton(RecordButtonStatus.isStopped);
        }

        private void InitialRecording()
        {
            backgroundWorker_manual = new BackgroundWorker();
            backgroundWorker_manual.DoWork += BackgroundWorker_Manual_DoWork;
            backgroundWorker_manual.RunWorkerCompleted += BackgroundWorker_Manual_RunWorkerCompleted;
        }

        private void InitialNotifyIcon()
        {
            Global._notifyIcon = new System.Windows.Forms.NotifyIcon();
            Global._notifyIcon.Icon = RMTV_recorder.Resource.icon;
            Global._notifyIcon.BalloonTipTitle = Parameter._windowTitle;
            Global._notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            Global._notifyIcon.Text = Parameter._windowTitle;
            Global._notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_MouseDoubleClick);
        }

        void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                Global._notifyIcon.ShowBalloonTip(2000);
                Global._notifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                Global._notifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        enum RecordButtonStatus
        {
            isStopped,
            isStartRecording,
            isRecording,
            isStopRecording,
            isStopped_Failed
        }

        private void SetRecordButton(RecordButtonStatus bStatus)
        {
            switch (bStatus)
            {
                case RecordButtonStatus.isStopped:
                    btn_record.ToolTip = "Start recording";
                    btn_record.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/Resource/record.png", UriKind.RelativeOrAbsolute)),
                        Stretch = Stretch.Uniform,
                        Height = 40,
                        Width = 40
                    };
                    label_loading.Visibility = Visibility.Hidden;
                    btn_record.IsEnabled = true;
                    btn_openlog.Visibility = Visibility.Visible;
                    label_status.Content = "Stopped";
                    clocktimer_manual.StopTimer();
                    cb_lang.IsEnabled = true;
                    break;

                case RecordButtonStatus.isStartRecording:
                    btn_record.ToolTip = "Start recording";
                    btn_record.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/Resource/record.png", UriKind.RelativeOrAbsolute)),
                        Stretch = Stretch.Uniform,
                        Height = 40,
                        Width = 40,
                        Opacity = 0.5
                    };
                    label_loading.Visibility = Visibility.Visible;
                    btn_record.IsEnabled = false;
                    btn_openlog.Visibility = Visibility.Hidden;
                    label_status.Content = "Starting...";
                    label_starttime.Content = DateTime.Now.ToString("HH:mm:ss");
                    cb_lang.IsEnabled = false;
                    clocktimer_manual.StartTimer();
                    break;

                case RecordButtonStatus.isRecording:
                    btn_record.ToolTip = "Stop recording";
                    btn_record.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/Resource/stop-record.png", UriKind.RelativeOrAbsolute)),
                        Stretch = Stretch.Uniform,
                        Height = 40,
                        Width = 40
                    };
                    label_loading.Visibility = Visibility.Hidden;
                    btn_record.IsEnabled = true;
                    btn_openlog.Visibility = Visibility.Hidden;
                    label_status.Content = "Recording...";
                    cb_lang.IsEnabled = false;
                    break;

                case RecordButtonStatus.isStopRecording:
                    btn_record.ToolTip = "Start recording";
                    btn_record.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/Resource/stop-record.png", UriKind.RelativeOrAbsolute)),
                        Stretch = Stretch.Uniform,
                        Height = 40,
                        Width = 40,
                        Opacity = 0.5
                    };
                    label_loading.Visibility = Visibility.Visible;
                    btn_record.IsEnabled = false;
                    btn_openlog.Visibility = Visibility.Hidden;
                    label_status.Content = "Stop Recording...";
                    cb_lang.IsEnabled = false;
                    break;

                case RecordButtonStatus.isStopped_Failed:
                    btn_record.ToolTip = "Start recording";
                    btn_record.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/Resource/record.png", UriKind.RelativeOrAbsolute)),
                        Stretch = Stretch.Uniform,
                        Height = 40,
                        Width = 40
                    };
                    label_loading.Visibility = Visibility.Hidden;
                    btn_record.IsEnabled = true;
                    btn_openlog.Visibility = Visibility.Visible;
                    label_status.Content = "Stopped (Failed)";
                    clocktimer_manual.StopTimer();
                    cb_lang.IsEnabled = true;
                    break;

                default:
                    break;
            }

        }

        private void DisableRecordButton(bool isDisabled)
        {
            btn_record.IsEnabled = !isDisabled;
        }

        private void btn_openfolder_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(Environment.CurrentDirectory, Parameter._outputPath);
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void btn_record_Click(object sender, RoutedEventArgs e)
        {
            isManuallyRecording = !isManuallyRecording;

            if (isManuallyRecording)
            {
                _selectedChannel_manual = cb_lang.Text;
                SetRecordButton(RecordButtonStatus.isStartRecording);
            }
            else
            {
                SetRecordButton(RecordButtonStatus.isStopRecording);
            }

            backgroundWorker_manual.RunWorkerAsync();
        }

        private void BackgroundWorker_Manual_DoWork(object sender, DoWorkEventArgs e)
        {
            if (isManuallyRecording)
            {
                RecordVideo();
                StartCheckManualRecordAlive();
                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                StopCheckManualRecordAlive();
                StopRecordVideo();
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void BackgroundWorker_Manual_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isManuallyRecording)
            {
                SetRecordButton(RecordButtonStatus.isRecording);
            }
            else
            {
                SetRecordButton(RecordButtonStatus.isStopped);
            }

        }
        
        private void RecordVideo()
        {
            ffmpeg_manual = new FFmpeg();
            ffmpeg_manual.StartRecord(_selectedChannel_manual);
        }

        private void StopRecordVideo()
        {
            if (ffmpeg_manual!= null)
            {
                ffmpeg_manual.StopRecord();
                ffmpeg_manual.Dispose();
            }
        }

        private void StartCheckManualRecordAlive()
        {
            timer_manualrecord_checkalive = new Timer();
            timer_manualrecord_checkalive.Elapsed += new ElapsedEventHandler(OnCheckAliveEvent);
            timer_manualrecord_checkalive.Interval = timer_manualrecord_checkalive_interval_sec * 1000;
            timer_manualrecord_checkalive.Enabled = true;
        }

        private void StopCheckManualRecordAlive()
        {
            if (timer_manualrecord_checkalive != null)
                timer_manualrecord_checkalive.Enabled = false;
        }

        private void OnCheckAliveEvent(object sender, EventArgs e)
        {
            if (!ffmpeg_manual.CheckAlive())
            {
                StopCheckManualRecordAlive();
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new OperationHandler(NotifyFailedRecording));
            }
        }

        public void NotifyFailedRecording()
        {
            isManuallyRecording = false;
            SetRecordButton(RecordButtonStatus.isStopped_Failed);
        }

        private void chechbox_isshutdown_Checked(object sender, RoutedEventArgs e)
        {
            chechbox_isshutdown.Foreground = (chechbox_isshutdown.IsChecked == true) ?
                                             Brushes.Crimson : Brushes.Black;
            chechbox_isshutdown.FontWeight = (chechbox_isshutdown.IsChecked == true) ?
                                             FontWeights.ExtraBold : FontWeights.Normal;
        }

        private void btn_openlog_m_Click(object sender, RoutedEventArgs e)
        {
            winCustom wincustom = new winCustom();
            Log_UC log_uc = new Log_UC();

            log_uc.Log = (ffmpeg_manual == null) ? "" : ffmpeg_manual.GetLog();
            log_uc.CloseDialog += new ucCustom.CloseDialogHandler(UserControl_CloseDialog);
            wincustom.winContent = log_uc;
            wincustom.Title = "Log";
            wincustom.Topmost = true;
            wincustom.Owner = this;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (isManuallyRecording || !IsAllSceduleCompleted())
            {
                if (MessageBox.Show(this, 
                    "Are you sure you want to exit? " + "\n" + "There are still videos recording or scheduled to record!", 
                    "Wait a minute!", 
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    cancelEventArgs.Cancel = true;
                    return;
                }
                else
                {
                    if (ffmpeg_manual != null) // manual one
                        ffmpeg_manual.KillProcess();

                    if (Global._scheduledRecObj.Count > 0) // scheduled ones
                    {
                        foreach (RecObj obj in Global._scheduledRecObj.RecObjs)
                        {
                            if (obj.Ffmpeg != null)
                                obj.Ffmpeg.KillProcess();

                            if (obj.Task != null)
                            {
                                obj.Task.CancelTask();
                            }
                        }
                    }
                }
            }

            ReleaseNotifyIcon();
        }

        private void ReleaseNotifyIcon()
        {
            if (Global._notifyIcon != null )
            {
                Global._notifyIcon.Dispose();
                Global._notifyIcon = null;
            }
        }

        private void Information_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            winCustom wincustom = new winCustom();
            Info_UC info_uc = new Info_UC();

            wincustom.winContent = info_uc;
            wincustom.Title = "Information";
            wincustom.Topmost = true;
            wincustom.Owner = this;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();

            RefreshDebugMode();
        }

        private void TV_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("This function hasn't done yet!");
        }

        private void TimeTable_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(Parameter.uri_RMTV_es);
        }

        private void Setting_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            winCustom wincustom = new winCustom();
            Setting_UC setting_uc = new Setting_UC();

            wincustom.winContent = setting_uc;
            wincustom.Title = "Setting";
            wincustom.Topmost = true;
            wincustom.Owner = this;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.ShowInTaskbar = false;

            if (wincustom.ShowDialog() == true)
            {
                RefreshSetting();
                IniHelper.WriteValue(Parameter._iniSectionSetting, Parameter._iniKeyTimeZoneId, Global._timezoneId, Parameter._setting_Path);
            }
            
        }

        private void RefreshSetting()
        {
            UpdateClock();
            clock.UpdateTimeZone(Global._timezoneId);
        }

        private void UpdateClock()
        {
            label_timezone.Content = "UTC " + CommonFunc.GetTimeZoneHour(Global._timezoneId);
            grid_clock.ToolTip = CommonFunc.GetTimeZoneDisplayName(Global._timezoneId);
        }

        static void UserControl_CloseDialog(object sender, bool bApply, EventArgs e)
        {

        }

        private void btn_addRec_Click(object sender, RoutedEventArgs e)
        {
            winCustom wincustom = new winCustom();
            AddRec_UC addRec_uc = new AddRec_UC();

            addRec_uc.CloseDialog += new ucCustom.CloseDialogHandler(UserControl_CloseDialog);
            wincustom.winContent = addRec_uc;
            wincustom.Title = "Schedule a recording";
            wincustom.Topmost = true;
            wincustom.Owner = this;
            wincustom.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wincustom.ShowInTaskbar = false;

            if (wincustom.ShowDialog() == true)
            {

                if (dgRecObj.Items.Count > 0)
                {
                    if (chechbox_isshutdown.Visibility != Visibility.Visible)
                    {
                        chechbox_isshutdown.Visibility = Visibility.Visible;
                        chechbox_isshutdown.IsChecked = false;
                        StartRefreshDataGrid();
                    }
                }
            }
        }

        private void btn_deleteRec_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecObj.SelectedItems.Count > 0)
            {
                int count = dgRecObj.SelectedItems.Count;
                for (int i = 0; i < count; i++)
                {
                    RecObj recObj = (RecObj)dgRecObj.SelectedItems[count -i -1];
                    if (recObj.Status == RecObj.RecordStatus.Recording ||
                        recObj.Status == RecObj.RecordStatus.Stopping)
                        continue;

                    recObj.Task.CancelTask();
                    recObj.Clean();
                    Global._scheduledRecObj.Remove(recObj);
                };
            }

            if (Global._scheduledRecObj.Count == 0)
            {
                chechbox_isshutdown.Visibility = Visibility.Hidden;
                StopRefreshDataGrid();
            }

        }

        private void btn_stopRec_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecObj.SelectedItems.Count > 0)
            {
                for (int i = 0; i < dgRecObj.SelectedItems.Count; i++)
                {
                    RecObj recObj = (RecObj)dgRecObj.SelectedItems[i];
                    if (recObj.Status == RecObj.RecordStatus.Recording)
                    {
                        recObj.IsStoppedManually = true;
                        recObj.Status = RecObj.RecordStatus.Stopping;
                        recObj.Ffmpeg.StopRecord();
                    }
                };
            }
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Adding 1 to make the row count start at 1 instead of 0
            // as pointed out by daub815
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void StartRefreshDataGrid()
        {
            if (timer_refreshdg != null)
            {
                if (!timer_refreshdg.Enabled)
                    timer_refreshdg.Enabled = true;
            }
            else
            {
                timer_refreshdg = new Timer();
                timer_refreshdg.Elapsed += new ElapsedEventHandler(OnRefreshEvent);
                timer_refreshdg.Interval = timer_refreshdg_interval_sec * 1000;
                timer_refreshdg.Enabled = true;
            }
        }

        private void StopRefreshDataGrid()
        {
            if (timer_refreshdg != null)
            {
                timer_refreshdg.Enabled = false;
            }
        }

        private void OnRefreshEvent(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new OperationHandler(RefreshDataGrid));
        }

        private void RefreshDataGrid()
        {
            //Debug.WriteLine("Refresh DataGrid at " + DateTime.Now.ToString("HH:mm:ss"));

            if (CommonFunc.GetStatusChangedFlag())
            {
                CommonFunc.ClearStatusChangedFlag();

                if (chechbox_isshutdown.IsChecked == true &&
                chechbox_isshutdown.Visibility == Visibility.Visible &&
                IsAllSceduleCompleted())
                {
                    StopRefreshDataGrid();
                    if (!CommonFunc.PrepareShutDown(this))
                    {
                        chechbox_isshutdown.IsChecked = false;
                        StartRefreshDataGrid();
                    }
                }
            }

        }

        private bool IsAllSceduleCompleted()
        {
            if (Global._scheduledRecObj.Count == 0)
            {
                return true;
            }
            else
            {
                if (Global._scheduledRecObj.Count == Global._scheduledRecObj.InactiveCount)
                    return true;
            }

            return false;
        }

        private void btn_ctrlC_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecObj.SelectedItems.Count > 0)
            {
                for (int i = 0; i < dgRecObj.SelectedItems.Count; i++)
                {
                    RecObj recObj = (RecObj)dgRecObj.SelectedItems[i];
                    recObj.IsStoppedManually = true;
                    recObj.Status = RecObj.RecordStatus.Stopping;
                    recObj.Ffmpeg.StopRecord();
                };
            }
        }

        private void btn_updateM3U8_Click(object sender, RoutedEventArgs e)
        {
            CommonFunc.RunWithProcessingUC(new OperationHandlerWithResult(CommonFunc.DownloadM3U8), "Downloading fils...");
        }

        private void btn_openlog_s_Click(object sender, RoutedEventArgs e)
        {
            RecObj obj = ((FrameworkElement)sender).DataContext as RecObj;

            winCustom wincustom = new winCustom();
            Log_UC log_uc = new Log_UC();

            log_uc.Log = (obj.Ffmpeg == null) ? "" : obj.Ffmpeg.GetLog();
            log_uc.CloseDialog += new ucCustom.CloseDialogHandler(UserControl_CloseDialog);
            wincustom.winContent = log_uc;
            wincustom.Title = "Log";
            wincustom.Topmost = true;
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();
        }

        private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            CommonFunc.test();
        }
    }
}
