using System;
using System.Diagnostics;
namespace Moonlight.Common.Tracing
{
	public interface IThreadSafeTracer
	{
		void TraceData(TraceEventType eventType, params object[] data);
		void DisableTracing();
		void EnableTracing();
		void AddTraceListener(TraceListener traceListener);
		void Close();
	}
}
