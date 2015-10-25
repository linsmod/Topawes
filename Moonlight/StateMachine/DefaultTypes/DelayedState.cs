using Moonlight.Common.Tracing;
using Moonlight.StateMachine.BaseTypes;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class DelayedState : BaseState
	{
		private readonly int minimumStateDuration;
		private readonly Stopwatch stopwatch;
		private Error error;
		private TransitionEventArgs transitionEventArgs;
		public DelayedState(int minimumStateDuration)
		{
			this.minimumStateDuration = ((minimumStateDuration >= 0) ? minimumStateDuration : 0);
			this.stopwatch = new Stopwatch();
		}
		public override void Start()
		{
			base.Start();
			this.error = null;
			this.stopwatch.Restart();
		}
		protected override void RaiseStateFinished(TransitionEventArgs eventArgs)
		{
			this.stopwatch.Stop();
			this.transitionEventArgs = eventArgs;
			if (this.stopwatch.ElapsedMilliseconds < (long)this.minimumStateDuration)
			{
				this.ExtendStateVisibility();
				return;
			}
			base.RaiseStateFinished(this.transitionEventArgs);
		}
		protected override void RaiseStateErrored(Error e)
		{
			this.stopwatch.Stop();
			this.error = e;
			if (this.stopwatch.ElapsedMilliseconds < (long)this.minimumStateDuration)
			{
				this.ExtendStateVisibility();
				return;
			}
			base.RaiseStateErrored(this.error);
		}
		private void ExtendStateVisibility()
		{
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.DoWork += new DoWorkEventHandler(this.ExtendStateVisibilityDoWork);
			backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ExtendStateVisibilityCompleted);
			backgroundWorker.RunWorkerAsync();
		}
		private void ExtendStateVisibilityDoWork(object sender, DoWorkEventArgs e)
		{
			int millisecondsTimeout = this.minimumStateDuration - (int)this.stopwatch.ElapsedMilliseconds;
			Thread.Sleep(millisecondsTimeout);
		}
		private void ExtendStateVisibilityCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				Tracer<DelayedState>.WriteError("Exception while waiting for delayed state to end!", new object[0]);
				Tracer<DelayedState>.WriteError(e.Error, e.Error.Message, new object[0]);
			}
			if (this.error != null)
			{
				base.RaiseStateErrored(this.error);
				return;
			}
			base.RaiseStateFinished(this.transitionEventArgs);
		}
	}
}
