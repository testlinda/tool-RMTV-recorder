using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace RMTV_recorder
{
    public partial class MainWindow : Window
    {
        private const string _windowTitle = "RMTV Recorder";
        private const string _outputPath = "output";
        private bool isManuallyRecording = false;
        private bool isScheduledRecording = false;
        private bool load_success = false;

        private BackgroundWorker backgroundWorker = null;
        private FFmpeg ffmpeg_manual = null;
        private Clock timer_manual = null;
        private string _selectedLang_manual = "Spanish";


        public MainWindow()
        {
            InitializeComponent();
            this.Title = _windowTitle;
            this.Loaded += Window_Loaded;

            load_success = Intialization();
            //test();
        }

        private void test()
        {
            MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory);
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
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
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
            isStopRecording
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

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (isManuallyRecording)
            {
                RecordVideo();
                Thread.Sleep(5000);
            }
            else
            {
                StopRecordVideo();
                Thread.Sleep(1000);
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            ffmpeg_manual.StartRecord(true, _selectedLang_manual.Equals("Spanish"));
        }

        private void StopRecordVideo()
        {
            if (ffmpeg_manual!= null)
            {
                ffmpeg_manual.StopRecord();
                string log = ffmpeg_manual.GetLog();
            }
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
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (isManuallyRecording || isScheduledRecording)
            {
                if (MessageBox.Show(this, 
                    "Are you sure you want to exit? " + "\n" + "There are still videos recording or scheduled to record!", 
                    "Wait a minute!", 
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    cancelEventArgs.Cancel = true;
                }
                else
                {
                    if (ffmpeg_manual != null) // manual one
                        ffmpeg_manual.KillProcess();

                    if (Global._groupRecObj.Count > 0) // scheduled ones
                    {
                        foreach (RecObj obj in Global._groupRecObj)
                        {
                            if (obj._ffmpeg != null)
                                obj._ffmpeg.KillProcess();
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
            wincustom.ShowInTaskbar = false;
            wincustom.ShowDialog();
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
            wincustom.ShowInTaskbar = false;

            if (wincustom.ShowDialog() == true)
            {
                dgRecObj.Items.Refresh();
                isScheduledRecording = true;
                chechbox_isshutdown.Visibility = Visibility.Visible;
            }
        }

        private void btn_deleteRec_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            if (dgRecObj.SelectedItems.Count > 0)
            {
                for (int i = dgRecObj.SelectedItems.Count; i > 0; i--)
                {
                    index = ((RecObj)dgRecObj.SelectedItems[i-1]).Index - 1;
                    Global._groupRecObj.RemoveAt(index);
                };
            }

            if (Global._groupRecObj.Count == 0)
            {
                isScheduledRecording = false;
                chechbox_isshutdown.Visibility = Visibility.Hidden;
            }

            RefreshRecObjIndex();
            dgRecObj.Items.Refresh();

            
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
            
        }

        
    }
}
