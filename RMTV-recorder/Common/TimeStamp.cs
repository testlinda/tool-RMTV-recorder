using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMTV_recorder
{
    class TimeStamp
    {
        public TimeStamp()
        {

        }

        public string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        }
    }
}
