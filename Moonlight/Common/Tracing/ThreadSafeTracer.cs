using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace Moonlight.Common.Tracing
{
	[CompilerGenerated]
	internal sealed class ThreadSafeTracer : IThreadSafeTracer
	{
		private readonly object syncRoot = new object();
		private readonly TraceSource tracer;
		public ThreadSafeTracer(string name, SourceLevels tracingLevel)
		{
			this.tracer = new TraceSource(name)
			{
				Switch = new SourceSwitch("Main switch")
				{
					Level = tracingLevel
				}
			};
		}
		public ThreadSafeTracer(string name) : this(name, SourceLevels.All)
		{
		}
		public void TraceData(TraceEventType eventType, params object[] data)
		{
			lock (this.syncRoot)
			{
				this.tracer.TraceData(eventType, 0, data);
			}
		}
		public void DisableTracing()
		{
			lock (this.syncRoot)
			{
				this.tracer.Switch.Level = SourceLevels.Off;
			}
		}
		public void EnableTracing()
		{
			lock (this.syncRoot)
			{
				this.tracer.Switch.Level = SourceLevels.All;
			}
		}
		public void AddTraceListener(TraceListener traceListener)
		{
			lock (this.syncRoot)
			{
				this.tracer.Listeners.Add(traceListener);
			}
		}
		public void Close()
		{
			lock (this.syncRoot)
			{
				this.tracer.Close();
			}
		}
	}
}
