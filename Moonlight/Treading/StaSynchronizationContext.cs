using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Moonlight.Treading
{
    public class StaSynchronizationContext : SynchronizationContext, IDisposable
    {
        private BlockingQueue<SendOrPostCallbackItem> mQueue;
        private StaThread mStaThread;
        public StaSynchronizationContext()
           : base()
        {
            mQueue = new BlockingQueue<SendOrPostCallbackItem>();
            mStaThread = new StaThread(mQueue);
            mStaThread.Start();
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            // create an item for execution
            SendOrPostCallbackItem item = new SendOrPostCallbackItem(d, state,
                                              ExecutionType.Send);
            // queue the item
            mQueue.Enqueue(item);
            // wait for the item execution to end
            item.ExecutionCompleteWaitHandle.WaitOne();

            // if there was an exception, throw it on the caller thread, not the
            // sta thread.
            if (item.ExecutedWithException)
                throw item.Exception;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            // queue the item and don't wait for its execution. This is risky because
            // an unhandled exception will terminate the STA thread. Use with caution.
            SendOrPostCallbackItem item = new SendOrPostCallbackItem(d, state,
                                              ExecutionType.Post);
            mQueue.Enqueue(item);
        }

        public void Dispose()
        {
            mStaThread.Stop();

        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}
