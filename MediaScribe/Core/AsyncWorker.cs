using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace JayDev.MediaScribe.Core
{
    /// <summary>
    /// Provides a single BackgroundWorker with its own work queue, and the ability to cancel the entire queue.
    /// This effectively follows the producer/consumer pattern.
    /// </summary>
    public class AsyncWorker
    {
        /// <summary>
        /// We use a BlockingCollection backed by a ConcurrentQueue, to not only ensure a thread-safe queue,
        /// but to enable us to call 'Take' and have it wait until a new work item is ready.
        /// </summary>
        BlockingCollection<Action<DoWorkEventArgs>> m_Queue = new BlockingCollection<Action<DoWorkEventArgs>>(new ConcurrentQueue<Action<DoWorkEventArgs>>());
        
        BackgroundWorker worker;
        
        /// <summary>
        /// Holds the DoWorkEventArgs for the current work item. This is needed to notify the BackgroundWorker
        /// to cancel its current task.
        /// </summary>
        DoWorkEventArgs args = new DoWorkEventArgs(null);

        public AsyncWorker(ThreadPriority threadPriority = ThreadPriority.AboveNormal)
        {
            //we create an above-normal priority background thread to manage the BackgroundWorker's
            //consumption. The reason we need this, is because we feed the BackgroundWorker its new
            //task in the RunWorkerCompletedEventHandler, which is executed on the thread that starts
            //the BackgroundWorker.
            var thread = new Thread(() =>
            {
                worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(ProcessNextWorkItem);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.RunWorkerAsync();
            });
            thread.Priority = threadPriority;
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// The BackgroundWorker's repeating job is to 'Take' the next workitem from the work queue.
        /// If there is no work item available, the 'Take' command will wait until there is.
        /// Once the operation is complete, the BackgroundWorker's RunWorkerCompleted event will re-start
        /// the BackgroundWorker, making it await the next work item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ProcessNextWorkItem(object sender, DoWorkEventArgs e)
        {
            Logging.Log(LoggingSource.AsyncWorker, "waiting for next workitem...");
            Action<DoWorkEventArgs> action = null;
            //If there is no item in the queue, due to the nature of the BlockingCollection it will wait until there is.
            action = m_Queue.Take();
            Logging.Log(LoggingSource.AsyncWorker, "work item read. performing workitem...");
            args = e;
            e.Cancel = false;
            action(e);
            Logging.Log(LoggingSource.AsyncWorker, "work item completed");
        }

        /// <summary>
        /// When the BackgroundWorker has completed its work item, re-start it - effectively
        /// instructing it to process the next work item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (null != e.Error)
            {
                throw e.Error;
            }
            worker.RunWorkerAsync();
        }

        public void QueueWorkItem(Action<DoWorkEventArgs> action)
        {
            lock (m_Queue)
            {
                Logging.Log(LoggingSource.AsyncWorker, "adding workitem...");
                m_Queue.Add(action);
            }
        }

        public void AbortAllWork()
        {
            lock (m_Queue)
            {
                Logging.Log(LoggingSource.AsyncWorker, "aborting all work...");

                while (m_Queue.Any(x => true))
                {
                    m_Queue.Take();
                }
                args.Cancel = true;
            }
        }
    }
}