using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
namespace Moonlight.Common.Tracing
{
	[CompilerGenerated]
	public static class Tracer<T>
	{
		private sealed class CurrentProcessInfo
		{
			public int Id;
			public string Name;
			private static Tracer<T>.CurrentProcessInfo instance;
			public static Tracer<T>.CurrentProcessInfo Instance
			{
				get
				{
					if (Tracer<T>.CurrentProcessInfo.instance == null)
					{
						Process currentProcess = Process.GetCurrentProcess();
						Tracer<T>.CurrentProcessInfo currentProcessInfo = new Tracer<T>.CurrentProcessInfo();
						currentProcessInfo.Id = currentProcess.Id;
						currentProcessInfo.Name = currentProcess.ProcessName;
						Tracer<T>.CurrentProcessInfo value = currentProcessInfo;
						Interlocked.CompareExchange<Tracer<T>.CurrentProcessInfo>(ref Tracer<T>.CurrentProcessInfo.instance, value, null);
					}
					return Tracer<T>.CurrentProcessInfo.instance;
				}
			}
		}
		internal static IThreadSafeTracer InternalTracer
		{
			get;
			set;
		}
		static Tracer()
		{
			Tracer<T>.InternalTracer = TraceManager.Instance.CreateTraceSource(typeof(T).FullName);
		}
		public static void LogEntry([CallerMemberName] string callerMemberName = "")
		{
			Tracer<T>.WriteEvent(TraceEventType.Information, null, string.Format("{0}()\tBEGIN", callerMemberName));
		}
		public static void LogExit([CallerMemberName] string callerMemberName = "")
		{
			Tracer<T>.WriteEvent(TraceEventType.Information, null, string.Format("{0}()\tEND", callerMemberName));
		}
		public static void WriteError(string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Error, null, string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteError(IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Error, null, string.Format(formatProvider, format, args));
		}
		public static void WriteError(Exception error)
		{
			Tracer<T>.WriteEvent(TraceEventType.Error, error.ToString(), null);
		}
		public static void WriteError(Exception error, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Error, error.ToString(), string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteError(Exception error, IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Error, error.ToString(), string.Format(formatProvider, format, args));
		}
		public static void WriteInformation(string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Information, null, string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteInformation(IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Information, null, string.Format(formatProvider, format, args));
		}
		public static void WriteInformation(Exception error, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Information, error.ToString(), string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteInformation(Exception error, IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Information, error.ToString(), string.Format(formatProvider, format, args));
		}
		public static void WriteVerbose(string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Verbose, null, string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteVerbose(IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Verbose, null, string.Format(formatProvider, format, args));
		}
		public static void WriteVerbose(Exception error)
		{
			Tracer<T>.WriteEvent(TraceEventType.Verbose, error.ToString(), null);
		}
		public static void WriteVerbose(Exception error, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Verbose, error.ToString(), string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteVerbose(Exception error, IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Verbose, error.ToString(), string.Format(formatProvider, format, args));
		}
		public static void WriteWarning(string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Warning, null, string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteWarning(IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Warning, null, string.Format(formatProvider, format, args));
		}
		public static void WriteWarning(Exception error)
		{
			Tracer<T>.WriteEvent(TraceEventType.Warning, error.ToString(), null);
		}
		public static void WriteWarning(Exception error, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Warning, error.ToString(), string.Format(CultureInfo.CurrentCulture, format, args));
		}
		public static void WriteWarning(Exception error, IFormatProvider formatProvider, string format, params object[] args)
		{
			Tracer<T>.WriteEvent(TraceEventType.Warning, error.ToString(), string.Format(formatProvider, format, args));
		}
		private static void WriteEvent(TraceEventType eventType, string errorText, string messageText)
		{
			string fileName = Path.GetFileName(typeof(T).Assembly.Location);
			Thread currentThread = Thread.CurrentThread;
			Tracer<T>.CurrentProcessInfo instance = Tracer<T>.CurrentProcessInfo.Instance;
			Tracer<T>.InternalTracer.TraceData(eventType, new object[]
			{
				instance.Id,
				instance.Name,
				currentThread.ManagedThreadId,
				currentThread.Name,
				fileName,
				messageText,
				errorText
			});
		}
	}
}
