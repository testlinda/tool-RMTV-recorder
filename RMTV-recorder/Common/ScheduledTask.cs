using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RMTV_recorder
{
    public class ScheduledTask
    {
        private OperationHandler _operation_delegate_1 = null;
        private OperationHandler _operation_delegate_2 = null;
        private DateTime _starttime = DateTime.MinValue;

        private CancellationTokenSource _tokenSource;
        private bool isTaskContinued = true;

        public ScheduledTask(DateTime starttime, OperationHandler operation)
        {
            _starttime = starttime;
            _operation_delegate_1 = operation;
            Initialization();
        }

        public ScheduledTask(DateTime starttime, OperationHandler operation1, OperationHandler operation2)
        {
            _starttime = starttime;
            _operation_delegate_1 = operation1;
            _operation_delegate_2 = operation2;
            Initialization();
        }

        private void Initialization()
        {
            _tokenSource = new CancellationTokenSource();
        }

        public bool ArrangeTask()
        {
            var dateNow = DateTime.Now;
            var date = _starttime;

            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = date.AddSeconds(Parameter.delay_sec);
                if (date > dateNow)
                    return false;

                ts = date - dateNow;
            }

            Task.Delay(ts, _tokenSource.Token).ContinueWith((isTaskContinued) => DoTask());
            return true;
        }

        private void DoTask()
        {
            if (!isTaskContinued)
                return;

            if (_operation_delegate_1 != null)
            {
                _operation_delegate_1();
            }

            if (_operation_delegate_2 != null)
            {
                _operation_delegate_2();
            }
        }

        public void CancelTask()
        {
            if (_tokenSource != null)
            {
                isTaskContinued = false;
                _tokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }
    }
}
