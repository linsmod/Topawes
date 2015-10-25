using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using WinFormsClient.Models;
using LiteDB;
using Moonlight;
using Moonlight.Treading;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient
{
    //public class Params
    //{
    //    public string Output { get; set; }
    //    public int CallCounter { get; set; }
    //    public int OriginalThread { get; set; }
    //}

    //class Program
    //{
    //    private static int mCount = 0;
    //    private static StaSynchronizationContext mStaSyncContext = null;
    //    [STAThread]
    //    static void Main(string[] args)
    //    {

    //        mStaSyncContext = new StaSynchronizationContext();
    //        for (int i = 0; i < 100; i++)
    //        {
    //            ThreadPool.QueueUserWorkItem(NonStaThread);

    //        }
    //        Console.WriteLine("Processing");
    //        Console.WriteLine("Press any key to dispose SyncContext");
    //        Console.ReadLine();
    //        mStaSyncContext.Dispose();
    //    }


    //    private static void NonStaThread(object state)
    //    {
    //        int id = Thread.CurrentThread.ManagedThreadId;

    //        for (int i = 0; i < 10; i++)
    //        {
    //            var param = new Params { OriginalThread = id, CallCounter = i };
    //            mStaSyncContext.Send(RunOnStaThread, param);
    //            Debug.Assert(param.Output == "Processed", "Unexpected behavior by STA thread");
    //        }
    //    }

    //    private static void RunOnStaThread(object state)
    //    {
    //        mCount++;
    //        Console.WriteLine(mCount);
    //        int id = Thread.CurrentThread.ManagedThreadId;
    //        var args = (Params)state;
    //        Trace.WriteLine("STA id " + id + " original thread " +
    //                        args.OriginalThread + " call count " + args.CallCounter);
    //        args.Output = "Processed";

    //    }
    //}
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //处理未捕获的异常   
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常   
            Application.ThreadException += Application_ThreadException;
            //处理非UI线程异常   
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //conn limit
            ServicePointManager.DefaultConnectionLimit = 10;

            //set webbrowser version
            IEFeatureControl.SetWebBrowserFeatures();

            Application.ApplicationExit += Application_ApplicationExit;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Application.Run(new WinFormsClient());
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            var sb = new StringBuilder();
            GetExceptions(sb, e.Exception);
            MessageBox.Show(sb.ToString(), Application.ProductName);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            AppSetting.Uninitialize();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            if (ex != null)
            {
                var sb = new StringBuilder();
                GetExceptions(sb, ex);
                MessageBox.Show(sb.ToString(), Application.ProductName);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            var sb = new StringBuilder();
            GetExceptions(sb, ex);
            MessageBox.Show(sb.ToString(), Application.ProductName);
        }



        public static void GetExceptions(StringBuilder sb, Exception ex)
        {
            sb.AppendLine("Message:" + ex.Message);
            sb.AppendLine("StackTrace:" + ex.StackTrace);
            if (ex.InnerException != null)
            {
                GetExceptions(sb, ex.InnerException);
            }
        }
    }
}
