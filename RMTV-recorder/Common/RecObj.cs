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

        public RecObj()
        {
            
        }

        public void Initialation()
        {
            Ffmpeg = new FFmpeg();
            Task = new ScheduledTask(CommonFunc.ConvertDateTime2Local(StartTime), 
                                     RecordVideo);
            Task.ArrangeTask();
        }

        private void RecordVideo()
        {
            if (Ffmpeg == null || Task == null)
            {
                Debug.WriteLine("RecObj was not initialized!");
                return;
            }
            Status = RecordStatus.Recording;
            Ffmpeg.StartRecord(false, (Language.Equals(Parameter.Language_Spanish)), Duration * 60);

            Status = (Ffmpeg.CheckFIleExist()) ? RecordStatus.Completed : RecordStatus.Failed;
            CommonFunc.RaiseCompleteFlag();
        }
    }
}
