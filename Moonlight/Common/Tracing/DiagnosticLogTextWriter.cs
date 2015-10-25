using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
namespace Moonlight.Common.Tracing
{
	public sealed class DiagnosticLogTextWriter : TextWriterTraceListener, ITraceWriter
	{
		private readonly object syncRoot = new object();
		private readonly string logFileName;
		public string LogFilePath
		{
			get;
			private set;
		}
		public DiagnosticLogTextWriter(string traceLogFolder, string filePrefix)
		{
			if (filePrefix == null)
			{
				throw new ArgumentNullException("filePrefix");
			}
			if (string.IsNullOrEmpty(filePrefix))
			{
				throw new ArgumentException("File prefix cannot be empty string.", "filePrefix");
			}
			this.logFileName = string.Format(CultureInfo.CurrentCulture, "{0}_{1:yyyy-MM-dd}_{1:HH mm}_{2}.log", new object[]
			{
				filePrefix,
				DateTime.Now,
				Path.GetRandomFileName()
			});
			this.ChangeLogFolder(traceLogFolder);
		}
		public void ChangeLogFolder(string newPath)
		{
			if (!Directory.Exists(newPath))
			{
				Directory.CreateDirectory(newPath);
			}
			lock (this.syncRoot)
			{
				try
				{
					if (base.Writer != null)
					{
						base.Writer.Close();
						base.Writer = null;
					}
				}
				catch (IOException ex)
				{
					Trace.WriteLine("DiagnosticLogTextWriter ChangeLogFolder catches:" + ex.Message);
				}
				this.LogFilePath = Path.Combine(newPath, this.logFileName);
				FileStream stream = new FileStream(this.LogFilePath, FileMode.Append, FileAccess.Write);
				base.Writer = new StreamWriter(stream)
				{
					AutoFlush = true
				};
			}
		}
		public override void Close()
		{
			lock (this.syncRoot)
			{
				if (base.Writer != null)
				{
					base.Writer.Close();
					base.Writer = null;
					this.LogFilePath = string.Empty;
				}
				base.Close();
			}
		}
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			this.TraceData(eventCache, source, eventType, (int)data[0], (string)data[1], (int)data[2], (string)data[3], (string)data[4], (string)data[5], (string)data[6]);
		}
		private static void AppendField(StringBuilder builder, object fieldValue)
		{
			if (builder.Length != 0)
			{
				builder.Append(" | ");
			}
			builder.Append(fieldValue ?? string.Empty);
		}
		private static void AppendField(StringBuilder builder, string formatString, params object[] args)
		{
			if (builder.Length != 0)
			{
				builder.Append(" | ");
			}
			builder.AppendFormat(CultureInfo.CurrentCulture, formatString, args);
		}
		private void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int processId, string processName, int threadId, string threadName, string assemblyFileName, string messageText, string errorText)
		{
			string formatString = eventCache.DateTime.ToLocalTime().ToString("u", CultureInfo.InvariantCulture);
			StringBuilder stringBuilder = new StringBuilder(250);
			DiagnosticLogTextWriter.AppendField(stringBuilder, formatString, new object[0]);
			DiagnosticLogTextWriter.AppendField(stringBuilder, "{0} ({1})", new object[]
			{
				processId,
				processName
			});
			if (threadName != null)
			{
				DiagnosticLogTextWriter.AppendField(stringBuilder, "0x{0:x8} ({1})", new object[]
				{
					threadId,
					threadName
				});
			}
			else
			{
				DiagnosticLogTextWriter.AppendField(stringBuilder, "0x{0:x8}", new object[]
				{
					threadId
				});
			}
			DiagnosticLogTextWriter.AppendField(stringBuilder, assemblyFileName, new object[0]);
			DiagnosticLogTextWriter.AppendField(stringBuilder, source, new object[0]);
			DiagnosticLogTextWriter.AppendField(stringBuilder, eventType);
			if (!string.IsNullOrEmpty(messageText))
			{
				messageText = messageText.Replace("{", "{{").Replace("}", "}}");
			}
			DiagnosticLogTextWriter.AppendField(stringBuilder, messageText);
			if (!string.IsNullOrEmpty(errorText))
			{
				errorText = errorText.Replace("{", "{{").Replace("}", "}}");
			}
			DiagnosticLogTextWriter.AppendField(stringBuilder, (errorText != null) ? ("<!CDATA[[" + errorText + "]]>") : string.Empty, new object[0]);
			lock (this.syncRoot)
			{
				if (base.Writer != null)
				{
					try
					{
						base.Writer.WriteLine(stringBuilder.ToString());
					}
					catch (IOException)
					{
					}
				}
			}
		}
	}
}
