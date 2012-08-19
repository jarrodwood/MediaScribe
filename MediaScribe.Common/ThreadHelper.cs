using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.ComponentModel;

namespace JayDev.MediaScribe.Common
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
            ExecuteSyncUI(dispatcher, action, DispatcherPriority.Normal);
        }

        public static void ExecuteSyncUI(Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            dispatcher.Invoke(action, priority);
        }


        public static void ExecuteBackground(Action action)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork+=new DoWorkEventHandler(delegate(object obj, DoWorkEventArgs args) { action(); });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    throw new ApplicationException("background worker failed... " + e.Error);
                }
            });
            worker.RunWorkerAsync();
        }

        public static void ExecuteBackground(Action action, Action callback)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object obj, DoWorkEventArgs args) { action(); });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object obj, RunWorkerCompletedEventArgs args) { callback(); });
            worker.RunWorkerAsync();
        }
    }
}
