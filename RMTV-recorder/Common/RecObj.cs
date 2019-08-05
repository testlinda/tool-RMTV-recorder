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

        public string Channel { get; set; }
        public string ChannelLink { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public string Log { get; set; }
        public FFmpeg Ffmpeg { get; set; }
        public ScheduledTask Task { get; set; }
        public string StrEndTime { get; set; }
        public string TimeZoneId { get; set; }
        public int RetryTimes { get; set; }
        public bool IsStoppedManually { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RecObj()
        {

        }

        public void Initialization()
        {
            Ffmpeg = new FFmpeg();
            StrStartTime = GetStrDateTime(StartTime);
            StrEndTime = GetStrDateTime(EndTime);
            IsStoppedManually = false;

            if (Status != RecordStatus.WaitForRetry)
            {
                Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(TimeZoneId, StartTime), Record);
            }
            else
            {
                Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(TimeZoneId, StartTime), RetryRecord);
            }

            Task.ArrangeTask();
        }

        public string GetStrDateTime(DateTime datetime)
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
            FinishRecord();
        }

        private void FinishRecord()
        {
            if (!IsStoppedManually && CheckRecordStoppedEarlier())
            {
                AddRetryRecord();
                Status = (Ffmpeg.CheckFIleExist()) ? RecordStatus.EndEarlier : RecordStatus.Failed;
            }
            else
            {
                Status = (Ffmpeg.CheckFIleExist()) ? RecordStatus.Completed : RecordStatus.Failed;
            }

            Global._scheduledRecObj.AddInactiveCount();
            Clean();
        }

        private bool CheckRecordStoppedEarlier()
        {
            if ((EndTime - CommonFunc.GetZoneTime(TimeZoneId)).TotalMinutes > Parameter.disconnection_diff_min)
            {
                return true;
            }
            return false;
        }

        private void AddRetryRecord()
        {
            if (RetryTimes >= Parameter.retry_times_limit)
                return;

            DateTime starttime = GetRetryStartTime();
            RecObj recObj = new RecObj
            {
                Channel = Channel,
                ChannelLink = ChannelLink,
                StartTime = starttime,
                EndTime = EndTime,
                Duration = GetDuration(),
                TimeZoneId = TimeZoneId,
                Status = RecObj.RecordStatus.WaitForRetry,
                Log = "",
                RetryTimes = this.RetryTimes + 1,
            };

            recObj.Initialization();
            Global._scheduledRecObj.Add(recObj);
        }

        private void RetryRecord()
        {
            if (!CommonFunc.CheckChannelLinkValid(Channel, ChannelLink))
            {
                Clean();
                StartTime = GetRetryStartTime();
                StrStartTime = GetStrDateTime(StartTime);
                Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(TimeZoneId, StartTime), RetryRecord);
                Task.ArrangeTask();
                return;
            }

            Status = RecordStatus.Recording;

            Ffmpeg.StartRecord(Channel, ChannelLink, Duration * 60, RetryTimes);
            FinishRecord();
        }

        private DateTime GetRetryStartTime()
        {
            Random rnd = new Random();
            int delay_sec = rnd.Next(Parameter.retry_wait_min_sec, Parameter.retry_wait_max_sec);
            return CommonFunc.GetZoneTime(TimeZoneId).AddSeconds(delay_sec);
        }

        private int GetDuration()
        {
            TimeSpan ts = EndTime - StartTime;
            double differenceInMinutes = ts.TotalMinutes;
            int duration = (int)differenceInMinutes;

            return duration;
        }

        public void Clean()
        {
            Ffmpeg.Dispose();
            Task.Dispose();
        }

    }

    public class ScheduledRecObj
    {
        public int InactiveCount { get; set; }
        public ObservableCollection<RecObj> RecObjs;
        public object SyncLock { get; set; }
        public int Count
        {
            get
            {
                return RecObjs.Count;
            }
        }

        public ScheduledRecObj()
        {
            InactiveCount = 0;
            RecObjs = new ObservableCollection<RecObj>();
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

                obj.Ffmpeg = null;
                obj.Task = null;
                RecObjs.Remove(obj);
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
