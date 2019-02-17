using System;
using System.Collections.Generic;
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
        private const string _windowTitle = "RMTV Recorder";
        private const string _outputPath = "output";
        private bool isManuallyRecording = false;
        private bool load_success = false;

        private BackgroundWorker backgroundWorker_manual = null;
        private FFmpeg ffmpeg_manual = null;
        private Clock timer_manual = null;
        private string _selectedLang_manual = Parameter.Language_Spanish ;
        private Timer timer_refreshdg = null;
        private int timer_refreshdg_interval_sec = 2;
        private Timer timer_manualrecord_checkalive = null;
        private int timer_manualrecord_checkalive_interval_sec = 2;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = _windowTitle;
            this.Loaded += Window_Loaded;

            load_success = Intialization();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!load_success)
            {
                Application.Current.Shutdown();
            }
        }

        private bool Intialization()
        {
            if (!CheckbinAvailable())
            {
                return false;
            }

            test();

            DisplayClock();
            InitialOutputFolder();
            InitialRecordButton();
            InitialUI();
            InitialRecording();
            InitialNotifyIcon();

            Global._groupRecObj = new List<RecObj>();
            dgRecObj.ItemsSource = Global._groupRecObj;
            Closing += OnClosing;

            return true;
        }

        private bool CheckbinAvailable()
        {
            if (File.Exists(Parameter._ffmpegPath) &&
                File.Exists(Parameter._m3u8_es_Path) &&
                File.Exists(Parameter._m3u8_en_Path) &&
                File.Exists(Parameter._keyfile_Path))
            {
                return true;
            }
            return false;
        }

        private void DisplayClock()
        {
            Clock timer_local = new Clock(this, label_time_local, true);
            timer_local.StartClock();
            Clock timer_spain = new Clock(this, label_time_spain, Parameter._timezoneIdSpain);
            timer_spain.StartClock();
        }

        private void InitialOutputFolder()
        {
            System.IO.Directory.CreateDirectory(_outputPath);
        }

        private void InitialRecordButton()
        {
            timer_manual = new Clock(this, label_duration);
            SetRecordButton(RecordButtonStatus.isStopped);
        }

        private void InitialUI()
        {
            chechbox_isshutdown.Visibility = Visibility.Hidden;
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
                    timer_manual.StopTimer();
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
                    timer_manual.StartTimer();
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
                    timer_manual.StopTimer();
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
            string path = Path.Combine(Environment.CurrentDirectory, _outputPath);
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
                _selectedLang_manual = cb_lang.Text;
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
            ffmpeg_manual.StartRecord(_selectedLang_manual.Equals(Parameter.Language_Spanish ));
        }

        private void StopRecordVideo()
        {
            if (ffmpeg_manual!= null)
            {
                ffmpeg_manual.StopRecord();
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

                    if (Global._groupRecObj.Count > 0) // scheduled ones
                    {
                        foreach (RecObj obj in Global._groupRecObj)
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

        private void Information_MouseDown(object sender, MouseButtonEventArgs e)
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
        }

        private void TV_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("This function hasn't done yet!");
        }

        private void TimeTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(Parameter.uri_RMTV_es);
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
                dgRecObj.Items.Refresh();

                if (dgRecObj.Items.Count == 1)
                {
                    chechbox_isshutdown.Visibility = Visibility.Visible;
                    chechbox_isshutdown.IsChecked = false;
                    StartRefreshDataGrid();
                }
            }
        }

        private void btn_deleteRec_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecObj.SelectedItems.Count > 0)
            {
                for (int i = 0; i < dgRecObj.SelectedItems.Count; i++)
                {
                    RecObj recObj = (RecObj)dgRecObj.SelectedItems[i];

                    if (recObj.Status == RecObj.RecordStatus.Recording ||
                        recObj.Status == RecObj.RecordStatus.Stopping)
                        continue;                   

                    recObj.Task.CancelTask();
                    Global._groupRecObj.Remove(recObj);
                };
            }

            if (Global._groupRecObj.Count == 0)
            {
                chechbox_isshutdown.Visibility = Visibility.Hidden;
                StopRefreshDataGrid();
            }

            RefreshRecObjIndex();
            dgRecObj.Items.Refresh();
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
                        recObj.Status = RecObj.RecordStatus.Stopping;
                        CommonFunc.RaiseStatusChangedFlag();
                        recObj.Ffmpeg.StopRecord();
                    }
                };
            }
        }

        private void StartRefreshDataGrid()
        {
            timer_refreshdg = new Timer();
            timer_refreshdg.Elapsed += new ElapsedEventHandler(OnRefreshEvent);
            timer_refreshdg.Interval = timer_refreshdg_interval_sec * 1000;
            timer_refreshdg.Enabled = true;
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
                dgRecObj.Items.Refresh();
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
            foreach (RecObj obj in Global._groupRecObj)
            {
                if (obj.Status == RecObj.RecordStatus.Recording || 
                    obj.Status == RecObj.RecordStatus.Scheduled ||
                    obj.Status == RecObj.RecordStatus.Stopping)
                {
                    return false;
                }
            }
            return true;
        }

        private void btn_test1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_test2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void test()
        {
            if (!File.Exists(Parameter._testfile_Path))
            {
                stackpanel_test.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshRecObjIndex()
        {
            foreach (RecObj obj in Global._groupRecObj)
            {
                obj.Index = Global._groupRecObj.IndexOf(obj) + 1;
            }
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

        
    }
}
