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
            WorkerThread.IsBackground = true;
            WorkerThread.Start();
        }
        public QueueItem<T> Current { get; private set; }
        public async void TheadLoop()
        {
            while (!Cts.IsCancellationRequested)
            {
                var queueItem = tasks.Dequeue();
                if (queueItem != null)
                {
                    var taskInfo = queueItem.Info;
                    lock (syncObject)
                    {
                        taskInfo = AppDatabase.db.TaskInfos.FindById(taskInfo.TaskId);
                        if (taskInfo.IsCanceled)
                        {
                            continue;
                        }
                        taskInfo.Running();
                        AppDatabase.db.TaskInfos.Update(taskInfo);
                    }
                    try
                    {
                        Current = queueItem;
                        await queueItem.Callback(queueItem.State);
                        taskInfo.Complete();
                    }
                    catch (Exception ex)
                    {
                        taskInfo.Fault(ex);
                    }
                    AppDatabase.db.TaskInfos.Update(taskInfo);
                    Current = null;

                }
                else
                {
                    await TaskEx.Delay(1000);
                }
            }
        }

        public TaskInfo CreateTaskItem(T input, Func<T, Task> method)
        {
            var queueItem = new QueueItem<T>
            {
                Info = new TaskInfo(),
                State = input,
                Callback = method
            };
            AppDatabase.db.TaskInfos.Insert(queueItem.Info);
            tasks.Enqueue(queueItem);
            return queueItem.Info;
        }

        public void CancelTaskItem(Guid taskId)
        {
            lock (syncObject)
            {
                var item = AppDatabase.db.TaskInfos.FindById(taskId);
                if (item != null && item.Status == TaskStatus.Created)
                {
                    item.Cancel();
                    AppDatabase.db.TaskInfos.Update(item);
                }
            }
        }


        public void Dispose()
        {
            tasks.ReleaseReader();
        }
    }

    public class AppQueue<T, TResult> : IDisposable
    {
        static object syncObject = new object();
        CancellationToken Cts;
        BlockingQueue<QueueItem<T, TResult>> tasks = new BlockingQueue<QueueItem<T, TResult>>();
        public Thread WorkerThread { get; set; }
        public AppQueue(CancellationToken token)
        {
            Cts = token;
            WorkerThread = new Thread(TheadLoop);
            WorkerThread.Start();
        }
        public QueueItem<T, TResult> Current { get; private set; }
        public async void TheadLoop()
        {
            while (!Cts.IsCancellationRequested)
            {
                var queueItem = tasks.Dequeue();
                if (queueItem != null)
                {
                    var taskInfo = queueItem.Info;
                    lock (syncObject)
                    {
                        taskInfo = AppDatabase.db.TaskInfos.FindById(taskInfo.TaskId);
                        if (taskInfo.IsCanceled)
                        {
                            continue;
                        }
                        taskInfo.Running();
                        AppDatabase.db.TaskInfos.Update(taskInfo);
                    }
                    try
                    {
                        Current = queueItem;
                        var result = await queueItem.Target(queueItem.State);
                        if (queueItem.Callback != null)
                            queueItem.Callback(result);
                        taskInfo.Complete();
                    }
                    catch (Exception ex)
                    {
                        taskInfo.Fault(ex);
                    }
                    AppDatabase.db.TaskInfos.Update(taskInfo);
                    Current = null;

                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public TaskInfo CreateTaskItem(T input, Func<T, Task<TResult>> method, Action<TResult> callback = null)
        {
            var queueItem = new QueueItem<T, TResult>
            {
                Info = new TaskInfo(),
                State = input,
                Target = method,
                Callback = callback
            };
            AppDatabase.db.TaskInfos.Insert(queueItem.Info);
            tasks.Enqueue(queueItem);
            return queueItem.Info;
        }

        public void CancelTaskItem(Guid taskId)
        {
            lock (syncObject)
            {
                var item = AppDatabase.db.TaskInfos.FindById(taskId);
                if (item != null && item.Status == TaskStatus.Created)
                {
                    item.Cancel();
                    AppDatabase.db.TaskInfos.Update(item);
                }
            }
        }


        public void Dispose()
        {
            tasks.ReleaseReader();
        }
    }


    public class QueueItem<T, TResult>
    {
        public Action<TResult> Callback { get; internal set; }
        public TaskInfo Info { get; set; }
        public T State { get; set; }
        public Func<T, Task<TResult>> Target { get; set; }
    }

    public class TaskInfo
    {
        public TaskInfo() :
            this(ObjectId.NewObjectId())
        {
        }
        public TaskInfo(ObjectId groupId)
        {
            TaskId = ObjectId.NewObjectId();
            GroupId = groupId;
            Status = TaskStatus.Created;
            CreateAt = DateTime.Now;
        }
        [BsonId]
        public ObjectId TaskId { get; set; }
        public ObjectId GroupId { get; set; }
        public bool IsCanceled
        {
            get
            {
                return this.Status == TaskStatus.Canceled;
            }
        }
        public bool IsCompleted
        {
            get
            {
                return this.Status == TaskStatus.RanToCompletion;
            }
        }

        public bool Faulted
        {
            get { return Status == TaskStatus.Faulted; }
        }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? CancelAt { get; set; }
        public DateTime? CompleteAt { get; set; }
        public DateTime? FaultAt { get; set; }
        public Exception Exception { get; set; }

        internal void Complete()
        {
            this.CompleteAt = DateTime.Now;
            this.Status = TaskStatus.RanToCompletion;
        }

        internal void Fault(Exception ex)
        {
            this.FaultAt = DateTime.Now;
            this.Status = TaskStatus.Faulted;
        }

        internal void Running()
        {
            this.Status = TaskStatus.Running;
        }

        internal void Cancel()
        {
            this.CancelAt = DateTime.Now;
            this.Status = TaskStatus.Canceled;
        }
    }

    public class QueueItem<T>
    {
        public TaskInfo Info { get; set; }
        public T State { get; set; }
        public Func<T, Task> Callback { get; set; }
    }
}
