using System;
using System.Diagnostics;
namespace Moonlight.Common.Tracing
{
	internal class TraceListenerAdapter : TraceListener
	{
		private readonly ITraceWriter writer;
		public TraceListenerAdapter(ITraceWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			this.writer = writer;
		}
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			this.writer.TraceData(eventCache, source, eventType, id, data);
		}
		public override void Write(string message)
		{
			throw new NotImplementedException();
		}
		public override void WriteLine(string message)
		{
			throw new NotImplementedException();
		}
	}
}
