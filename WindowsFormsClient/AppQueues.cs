using LiteDB;
using Moonlight.Treading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinFormsClient
{
    public class AppQueue<T> : IDisposable
    {
        static object syncObject = new object();
        CancellationToken Cts;
        BlockingQueue<QueueItem<T>> tasks = new BlockingQueue<QueueItem<T>>();
        public Thread WorkerThread { get; set; }
        public AppQueue(CancellationToken token)
        {
            Cts = token;
            WorkerThread = new Thread(TheadLoop);
            WorkerThread.Start();
        }
        async void TheadLoop()
        {
            while (!Cts.IsCancellationRequested)
            {
                var queueItem = tasks.Dequeue();
                if (queueItem != null)
                {
                    try
                    {
                        await queueItem.Target(queueItem.State);
                    }
                    catch (Exception ex)
                    {
                        queueItem.Callback(queueItem.State, ex);
                    }
                }
            }
        }

        public void Enqueue(T input, Func<T, Task> method, Action<T, Exception> callback)
        {
            var queueItem = new QueueItem<T>
            {
                State = input,
                Target = method,
                Callback = callback
            };
            tasks.Enqueue(queueItem);
        }


        public void Dispose()
        {
            tasks.ReleaseReader();
        }
    }

    public class TaskQueue<T, TResult> : IDisposable
    {
        static object syncObject = new object();
        CancellationToken Cts;
        BlockingQueue<QueueItem<T, TResult>> tasks = new BlockingQueue<QueueItem<T, TResult>>();
        public Thread WorkerThread { get; set; }
        public TaskQueue(CancellationToken token)
        {
            Cts = token;
            WorkerThread = new Thread(TheadLoop);
            WorkerThread.Name = string.Format("任务队列 TaskQueue<{0},{1}>", typeof(T).Name, typeof(TResult).Name);
            WorkerThread.Start();
        }
        async void TheadLoop()
        {
            while (!Cts.IsCancellationRequested)
            {
                var queueItem = tasks.Dequeue();
                if (queueItem != null)
                {
                    try
                    {
                        var result = await queueItem.Target(queueItem.Input);
                        if (queueItem.Callback != null)
                            queueItem.Callback(queueItem.Input, result, null);
                    }
                    catch (Exception ex)
                    {
                        if (queueItem.Callback != null)
                            queueItem.Callback(queueItem.Input, default(TResult), ex);
                    }
                }
            }
        }

        public void Enqueue(T input, Func<T, Task<TResult>> method)
        {
            Enqueue(input, method, null, CancellationToken.None);
        }

        public void Enqueue(T input, Func<T, Task<TResult>> method, Action<T, TResult, Exception> callback)
        {
            Enqueue(input, method, callback, CancellationToken.None);
        }

        public void Enqueue(T input, Func<T, Task<TResult>> method, Action<T, TResult, Exception> callback, CancellationToken cancelToken)
        {
            var queueItem = new QueueItem<T, TResult>
            {
                Input = input,
                Target = method,
                Callback = callback,
                CancelToken = cancelToken
            };
            tasks.Enqueue(queueItem);
        }


        public void Dispose()
        {
            tasks.ReleaseReader();
        }
    }


    public class QueueItem<T, TResult>
    {
        public Action<T, TResult, Exception> Callback { get; internal set; }
        public CancellationToken CancelToken { get; internal set; }
        public T Input { get; set; }
        public Func<T, Task<TResult>> Target { get; set; }
    }

    public class QueueItem<T>
    {
        public Action<T, Exception> Callback { get; internal set; }
        public T State { get; set; }
        public Func<T, Task> Target { get; set; }
    }
}
