using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace Moonlight.Common.Tracing
{
	public class TraceManager
	{
		private static readonly object StaticSyncRoot = new object();
		private static TraceManager instance;
		private readonly object syncRoot = new object();
		private readonly string defaultLogFolder;
		private SourceLevels currentTracingLevel;
		public static TraceManager Instance
		{
			get
			{
				if (TraceManager.instance == null)
				{
					lock (TraceManager.StaticSyncRoot)
					{
						if (TraceManager.instance == null)
						{
							TraceManager.instance = new TraceManager();
						}
					}
				}
				return TraceManager.instance;
			}
		}
		internal ITraceWriter MainTraceWriter
		{
			get;
			private set;
		}
		internal List<IThreadSafeTracer> Tracers
		{
			get;
			private set;
		}
		internal TraceManager()
		{
			this.defaultLogFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Microsoft\\Care Suite\\Windows Device Recovery Tool\\Traces\\";
			this.currentTracingLevel = SourceLevels.All;
			this.Tracers = new List<IThreadSafeTracer>();
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.OnCurrentDomainProcessExit);
		}
		private void RegisterDiagnosticTraceWriter(string logPath, string logNamePrefix)
		{
			ITraceWriter traceWriter = new DiagnosticLogTextWriter(logPath, logNamePrefix);
			lock (this.syncRoot)
			{
				try
				{
					if (this.MainTraceWriter != null)
					{
						this.MainTraceWriter.Close();
					}
					TraceListener traceListener = traceWriter as TraceListener;
					foreach (IThreadSafeTracer current in this.Tracers)
					{
						current.AddTraceListener(traceListener);
					}
					this.MainTraceWriter = traceWriter;
					Tracer<TraceManager>.WriteInformation("New diagnostic trace writer registered.", new object[0]);
				}
				catch (Exception ex)
				{
					Tracer<TraceManager>.WriteError(ex, "Could not register diagnostic trace writer.", new object[0]);
					throw new InvalidOperationException("Could not register diagnostic trace writer.", ex);
				}
			}
		}
		public void EnableDiagnosticLogs(string logPath, string logNamePrefix)
		{
			if (this.MainTraceWriter == null)
			{
				this.RegisterDiagnosticTraceWriter(logPath, logNamePrefix);
			}
			lock (this.syncRoot)
			{
				if (this.MainTraceWriter == null)
				{
					throw new InvalidOperationException("RegisterDiagnosticTraceWriter must be called before using this method.");
				}
				try
				{
					if (string.IsNullOrEmpty(this.MainTraceWriter.LogFilePath))
					{
						this.MainTraceWriter.ChangeLogFolder(this.defaultLogFolder);
					}
					foreach (IThreadSafeTracer current in this.Tracers)
					{
						current.EnableTracing();
					}
					this.currentTracingLevel = SourceLevels.All;
					Tracer<TraceManager>.WriteInformation("Diagnostic logs enabled.", new object[0]);
				}
				catch (Exception ex)
				{
					Tracer<TraceManager>.WriteError(ex, "Could not enable diagnostic logs.", new object[0]);
					throw new InvalidOperationException("Could not enable diagnostic logs.", ex);
				}
			}
		}
		public void DisableDiagnosticLogs(bool removeCurrentLogFile)
		{
			lock (this.syncRoot)
			{
				if (this.MainTraceWriter == null)
				{
					throw new InvalidOperationException("RegisterDiagnosticTraceWriter must be called before using this method.");
				}
				try
				{
					if (removeCurrentLogFile && !string.IsNullOrEmpty(this.MainTraceWriter.LogFilePath))
					{
						string logFilePath = this.MainTraceWriter.LogFilePath;
						this.MainTraceWriter.Close();
						File.Delete(logFilePath);
						this.MainTraceWriter = null;
					}
					foreach (IThreadSafeTracer current in this.Tracers)
					{
						current.DisableTracing();
					}
					this.currentTracingLevel = SourceLevels.Off;
					Tracer<TraceManager>.WriteInformation("Diagnostic logs disabled.", new object[0]);
				}
				catch (Exception ex)
				{
					Tracer<TraceManager>.WriteError(ex, "Could not disable diagnostic logs.", new object[0]);
					throw new InvalidOperationException("Could not disable diagnostic logs.", ex);
				}
			}
		}
		public void ChangeDiagnosticLogFolder(string newPath)
		{
			lock (this.syncRoot)
			{
				if (this.MainTraceWriter == null)
				{
					throw new InvalidOperationException("RegisterDiagnosticTraceWriter must be called before using this method.");
				}
				try
				{
					this.MainTraceWriter.ChangeLogFolder(newPath);
					Tracer<TraceManager>.WriteInformation("Diagnostic logs folder changed.", new object[0]);
				}
				catch (Exception ex)
				{
					Tracer<TraceManager>.WriteError(ex, "Could not change diagnostic logs folder.", new object[0]);
					throw new InvalidOperationException("Could not change diagnostic logs folder.", ex);
				}
			}
		}
		public void RemoveDiagnosticLogs(string directoryPath, string appNamePrefix, bool traceEnabled)
		{
			Tracer<TraceManager>.WriteInformation("Remove diagnostic logs.", new object[0]);
			lock (this.syncRoot)
			{
				string[] files = Directory.GetFiles(directoryPath);
				string[] array = files;
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					try
					{
						File.Delete(text);
						Tracer<TraceManager>.WriteInformation("Succesfully removed file: {0}.", new object[]
						{
							text
						});
					}
					catch (Exception error)
					{
						Tracer<TraceManager>.WriteError(error, "Following file could not be deleted: {0}.", new object[]
						{
							text
						});
					}
				}
			}
			if (!traceEnabled && this.MainTraceWriter != null)
			{
				this.DisableDiagnosticLogs(true);
			}
			Tracer<TraceManager>.WriteInformation("Finished removing diagnostic logs.", new object[0]);
		}
		public void MoveDiagnosticLogFile(string newPath)
		{
			lock (this.syncRoot)
			{
				if (this.MainTraceWriter == null)
				{
					throw new InvalidOperationException("RegisterDiagnosticTraceWriter must be called before using this method.");
				}
				if (string.IsNullOrEmpty(this.MainTraceWriter.LogFilePath))
				{
					throw new InvalidOperationException("Current diagnostic log file does not exist. There is nothing to be moved.");
				}
				try
				{
					string logFilePath = this.MainTraceWriter.LogFilePath;
					this.MainTraceWriter.Close();
					Directory.CreateDirectory(newPath);
					File.Move(logFilePath, Path.Combine(newPath, Path.GetFileName(logFilePath)));
					this.MainTraceWriter.ChangeLogFolder(newPath);
					Tracer<TraceManager>.WriteInformation("Diagnostic logs folder changed.", new object[0]);
				}
				catch (Exception ex)
				{
					Tracer<TraceManager>.WriteError(ex, "Could not move diagnostic logs file", new object[0]);
					throw new InvalidOperationException("Could not move diagnostic logs file.", ex);
				}
			}
		}
		internal IThreadSafeTracer CreateTraceSource(string sourceName)
		{
			IThreadSafeTracer result;
			try
			{
				lock (this.syncRoot)
				{
					ThreadSafeTracer threadSafeTracer = new ThreadSafeTracer(sourceName, this.currentTracingLevel);
					if (this.MainTraceWriter != null)
					{
						TraceListener traceListener = this.MainTraceWriter as TraceListener;
						if (traceListener == null)
						{
							traceListener = new TraceListenerAdapter(this.MainTraceWriter);
						}
						threadSafeTracer.AddTraceListener(traceListener);
					}
					this.Tracers.Add(threadSafeTracer);
					result = threadSafeTracer;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Could not create new tracer. Error: " + ex.Message);
				throw new InvalidOperationException("Could not create new tracer.", ex);
			}
			return result;
		}
		private void OnCurrentDomainProcessExit(object sender, EventArgs e)
		{
			try
			{
				lock (this.syncRoot)
				{
					if (this.MainTraceWriter != null)
					{
						this.MainTraceWriter.Close();
					}
					using (List<IThreadSafeTracer>.Enumerator enumerator = this.Tracers.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ThreadSafeTracer threadSafeTracer = (ThreadSafeTracer)enumerator.Current;
							threadSafeTracer.Close();
						}
					}
					this.Tracers.Clear();
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine("TraceManager OnCurrentDomainProcessExit catches:" + ex.Message);
				throw;
			}
		}
	}
}
