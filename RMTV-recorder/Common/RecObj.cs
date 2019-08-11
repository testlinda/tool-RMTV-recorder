using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RMTV_recorder
{
    public class RecObj : INotifyPropertyChanged
    {
        public enum RecordStatus
        {
            Scheduled = 0,
            //
            Recording = 1,
            Stopping = 2,
            WaitForRetry = 3,
            //
            Completed = 4,
            EndEarlier = 5,
            Failed = 6,
        }

        private RecordStatus _status;
        public RecordStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                NotifyPropertyChanged();
                CommonFunc.RaiseStatusChangedFlag();
                if (_status >= RecordStatus.Completed)
                    GlobalVar._RecObjs.AddInactiveCount();
            }
        }

        private DateTime _starttime;
        public DateTime StartTime
        {
            get
            {
                return _starttime;
            }
            set
            {
                _starttime = value;
                NotifyPropertyChanged();
            }
        }

        private string _strStartTime;
        public string StrStartTime
        {
            get
            {
                return _strStartTime;
            }
            set
            {
                _strStartTime = value;
                NotifyPropertyChanged();
            }
        }

        private int _duration;
        public int Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
                NotifyPropertyChanged();
            }
        }

        public string Channel { get; set; }
        public string ChannelLink { get; set; }
        public DateTime EndTime { get; set; }
        public string Log { get; set; }
        public FFmpeg Ffmpeg { get; set; }
        public ScheduledTask Task { get; set; }
        public string StrEndTime { get; set; }
        public string TimeZoneId { get; set; }
        public int RetryTimes { get; set; }

        private bool IsStoppedManually { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RecObj()
        {
            Debug.WriteLine("RecObj Created.");

            Ffmpeg = new FFmpeg();
        }

        public void Initialization()
        {
            Debug.WriteLine("RecObj Initialed.");

            StrStartTime = GetStrDateTime(StartTime);
            StrEndTime = GetStrDateTime(EndTime);
            OperationHandler handler = (Status != RecordStatus.WaitForRetry) ? 
                                       (OperationHandler)Record : (OperationHandler)RetryRecord;
            Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(TimeZoneId, StartTime), handler);
            Task.ArrangeTask();
        }

        public void StopRecord()
        {
            IsStoppedManually = true;
            Status = RecordStatus.Stopping;
            Ffmpeg.StopRecord();
        }

        public string GetLog()
        {
            if (Ffmpeg != null)
            {
                Log = Ffmpeg.GetLog();
            }
            return Log;
        }

        public void Dispose()
        {
            Debug.WriteLine("RecObj Dispose.");
            if (Ffmpeg != null)
            {
                Ffmpeg.Dispose();
                Ffmpeg = null;
            }

            if (Task != null)
            {
                Task.Dispose();
                Task = null;
            }
        }

        private string GetStrDateTime(DateTime datetime)
        {
            string strFormat = "yyyy/MM/dd a\\t HH:mm:ss";
            string strTime = String.Concat("(",
                                           CommonFunc.GetTimeZoneHour(TimeZoneId),
                                            "):\t",
                                           datetime.ToString(strFormat),
                                           "\r\n",
                                           "Local:\t",
                                           CommonFunc.ConvertDateTime2Local(TimeZoneId, datetime).ToString(strFormat));
            return strTime;
        }

        private void Record()
        {
            if (Ffmpeg == null || Task == null)
            {
                Debug.WriteLine("RecObj was not initialized!");
                return;
            }
            Status = RecordStatus.Recording;
            Ffmpeg.StartRecord(Channel, ChannelLink, Duration * 60, RetryTimes);
            Finish();
        }

        private void Finish()
        {
            if (!Ffmpeg.IsFIleExist())
            {
                Status = RecordStatus.Failed;
            }
            else
            {
                DateTime currenttime = CommonFunc.GetZoneTime(TimeZoneId);
                if (!IsStoppedManually && !IsNearTheEndTime(currenttime))
                {
                    AddRetryRecord();
                    Status = RecordStatus.EndEarlier;
                }
                else
                {
                    Status = RecordStatus.Completed;
                }
            }

            GetLog();
            Dispose();
        }

        private void AddRetryRecord()
        {
            if (RetryTimes >= Parameter.retry_times_limit)
                return;

            DateTime starttime = GetRetryStartTime();
            if (IsNearTheEndTime(starttime))
            {
                return;
            }

            RecObj recObj = new RecObj
            {
                Channel = Channel,
                ChannelLink = ChannelLink,
                StartTime = starttime,
                EndTime = EndTime,
                Duration = GetRetryDuration(starttime, EndTime),
                TimeZoneId = TimeZoneId,
                Status = RecObj.RecordStatus.WaitForRetry,
                Log = "",
                RetryTimes = this.RetryTimes + 1,
            };

            recObj.Initialization();
            GlobalVar._RecObjs.Add(recObj);
        }

        private void RetryRecord()
        {
            if (Ffmpeg == null || Task == null)
            {
                Debug.WriteLine("RecObj was not initialized!");
                return;
            }

            if (CommonFunc.CheckChannelLinkValid(Channel, ChannelLink))
            {
                Status = RecordStatus.Recording;
                Ffmpeg.StartRecord(Channel, ChannelLink, Duration * 60, RetryTimes);
                Finish();
            }
            else
            {
                DateTime temp_starttime = GetRetryStartTime();
                if (IsNearTheEndTime(temp_starttime))
                {
                    Status = RecordStatus.Failed;
                }
                else
                {
                    StartTime = temp_starttime;
                    Duration = GetRetryDuration(StartTime, EndTime);
                    Initialization();
                }
            }            
        }

        private bool IsNearTheEndTime(DateTime time)
        {
            DateTime endtime_with_diff = EndTime.AddMinutes((-1) * Parameter.disconnection_diff_min);
            if (time > endtime_with_diff)
            {
                return true;
            }
            return false;
        }

        private DateTime GetRetryStartTime()
        {
            Random rnd = new Random();
            int delay_sec = rnd.Next(Parameter.retry_wait_min_sec, Parameter.retry_wait_max_sec);
            return CommonFunc.GetZoneTime(TimeZoneId).AddSeconds(delay_sec);
        }

        private int GetRetryDuration(DateTime starttime, DateTime endtime)
        {
            TimeSpan ts = endtime - starttime;
            double differenceInMinutes = ts.TotalMinutes;
            int duration = (int)differenceInMinutes;

            return duration;
        }
    }

    public class RecObjCollection
    {        
        public ObservableCollection<RecObj> RecObjs;

        public int InactiveCount { get; set; }
        public object SyncLock { get; set; }
        public int Count
        {
            get
            {
                return RecObjs.Count;
            }
        }

        public RecObjCollection()
        {
            RecObjs = new ObservableCollection<RecObj>();
            InactiveCount = 0;
            SyncLock = new object();
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(RecObjs, SyncLock);
        }

        public void Add(RecObj obj)
        {
            lock (SyncLock)
            {
                RecObjs.Add(obj);
            }
        }

        public void Remove(RecObj obj)
        {
            lock (SyncLock)
            {
                if (obj.Status >= RecObj.RecordStatus.Completed)
                    InactiveCount--;

                RecObjs.Remove(obj);
                obj.Dispose();
                obj = null;
            }    
        }

        public void AddInactiveCount()
        {
            lock(SyncLock)
            {
                InactiveCount++;
            }
        }
    }
}
