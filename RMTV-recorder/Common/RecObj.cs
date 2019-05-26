using System;
using System.Collections.Generic;
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
            Scheduled,
            //
            Recording,
            Stopping,
            //
            Completed,
            EndEarlier,
            Failed
        }

        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                NotifyPropertyChanged();
            }
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

        public string Channel { get; set; }
        public string ChannelLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public string Log { get; set; }
        public FFmpeg Ffmpeg { get; set; }
        public ScheduledTask Task { get; set; }
        public string StrStartTime { get; set; }
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
            Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(TimeZoneId, StartTime), RecordVideo);
            Task.ArrangeTask();
            StrStartTime = GetStrDateTime(StartTime);
            StrEndTime = GetStrDateTime(EndTime);
            IsStoppedManually = false;
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

        private void RecordVideo()
        {
            if (Ffmpeg == null || Task == null)
            {
                Debug.WriteLine("RecObj was not initialized!");
                return;
            }
            Status = RecordStatus.Recording;

            Ffmpeg.StartRecord(Channel, ChannelLink, Duration * 60, RetryTimes);

            if (!IsStoppedManually && CheckRecordStoppedEarlier())
            {
                RetryRecordVideo();
                Status = (Ffmpeg.CheckFIleExist()) ? RecordStatus.EndEarlier : RecordStatus.Failed;
            }
            else
            {
                Status = (Ffmpeg.CheckFIleExist()) ? RecordStatus.Completed : RecordStatus.Failed;
            }

        }

        private bool CheckRecordStoppedEarlier()
        {
            if ((EndTime - CommonFunc.GetZoneTime(TimeZoneId)).TotalMinutes > Parameter.disconnection_diff_min)
            {
                return true;
            }
            return false;
        }

        private void RetryRecordVideo()
        {
            if (RetryTimes >= Parameter.retry_times_limit)
                return;

            DateTime starttime = GetRetryStartTime();
            if (starttime >= EndTime)
                return;

            RecObj recObj = new RecObj
            {
                Index = GetIndex(),
                Channel = Channel,
                ChannelLink = ChannelLink,
                StartTime = starttime,
                EndTime = EndTime,
                Duration = GetDuration(),
                TimeZoneId = Global._timezoneId,
                Status = RecObj.RecordStatus.Scheduled,
                Log = "",
                RetryTimes = this.RetryTimes + 1,
            };

            recObj.Initialization();
            CommonFunc.AddRecObj(recObj);
        }

        private int GetIndex()
        {
            return Global._groupRecObj.Count + 1;
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

    }
}
