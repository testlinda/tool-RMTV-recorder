using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RMTV_recorder
{
    public class RecObj
    {
        public enum RecordStatus
        {
            Scheduled,
            Recording,
            Stopping,
            Completed,
            Failed
        }

        public int Index { get; set; }
        public string Language { get; set; }
        public DateTime StartTime { get; set; }
        public bool StartTimeIsNow { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public RecordStatus Status { get; set; }
        public string Log { get; set; }
        public FFmpeg Ffmpeg { get; set; }
        public ScheduledTask Task { get; set; }
        public string StrStartTime { get; set; }
        public string StrEndTime { get; set; }

        public RecObj()
        {
            
        }

        public void Initialization()
        {
            Ffmpeg = new FFmpeg();
            Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(StartTime), RecordVideo);
            Task.ArrangeTask();
            StrStartTime = GetStrDateTime(StartTime);
            StrEndTime = GetStrDateTime(EndTime);
        }

        public string GetStrDateTime(DateTime datetime)
        {
            string strFormat = "yyyy/MM/dd a\\t HH:mm:ss";
            string strTime = String.Concat("Spain:\t", 
                                           datetime.ToString(strFormat),
                                           "\r\n",
                                           "Local:\t",
                                           CommonFunc.ConvertDateTime2Local(datetime).ToString(strFormat));
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
            CommonFunc.RaiseStatusChangedFlag();

            Ffmpeg.StartRecord((Language.Equals(Parameter.Language_Spanish)), Duration * 60);

            //System.Threading.Thread.Sleep(3000);
            Status = (Ffmpeg.CheckFIleExist()) ? RecordStatus.Completed : RecordStatus.Failed;
            CommonFunc.RaiseStatusChangedFlag();
        }
    }
}
