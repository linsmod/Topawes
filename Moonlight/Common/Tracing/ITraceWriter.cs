using System;
using System.Diagnostics;
namespace Moonlight.Common.Tracing
{
	public interface ITraceWriter
	{
		string LogFilePath
		{
			get;
		}
		void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data);
		void ChangeLogFolder(string newPath);
		void Close();
	}
}
