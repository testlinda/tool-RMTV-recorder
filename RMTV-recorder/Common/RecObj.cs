using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMTV_recorder
{
    class RecObj
    {
        public enum RecordStatus
        {
            WillBeRecorded,
            IsRecording,
            RecordCompleted,
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
        public FFmpeg _ffmpeg = null;

        //public RecObj()
        //{

        //}

        //protected int sortIndex()
        //{

        //}
    }
}
