using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace JayDev.Notemaker.Common
{
    public enum ExecuteBehaviour { Sync, Async }
    public static class ThreadHelper
    {

        public static void ExecuteAsyncUI(Dispatcher dispatcher, Action action, Action callback)
        {
            DispatcherOperation dispatcherOp = dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
            if (null != callback)
            {
                dispatcherOp.Completed += (s, e) => { callback(); };
            }
        }

        public static void ExecuteAsyncUI(Dispatcher dispatcher, Action action)
        {
            ExecuteAsyncUI(dispatcher, action, null);
        }

        public static void ExecuteSyncUI(Dispatcher dispatcher, Action action)
        {
            dispatcher.Invoke(action, DispatcherPriority.Normal);
        }


        public static void ExecuteBackground(Action action)
        {
            throw new NotImplementedException();
        }

        public static void ExecuteBackground(Action action, Action callback)
        {
            throw new NotImplementedException();
        }
    }
}
