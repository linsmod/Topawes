using Moonlight.Common.Tracing;
using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public abstract class StateMachineState : BaseState
	{
		public new bool Started
		{
			get;
			private set;
		}
		public BaseState CurrentState
		{
			get
			{
				if (this.Machine == null)
				{
					return null;
				}
				return this.Machine.CurrentState;
			}
		}
		protected BaseStateMachine Machine
		{
			get;
			set;
		}
		protected StateMachineState()
		{
			this.Machine = new BaseStateMachine
			{
				MachineName = this.ToString()
			};
			this.Machine.CurrentStateChanged += new Action<BaseState, BaseState>(this.OnCurrentStateChanged);
		}
		public sealed override string ToString()
		{
			return base.ToString();
		}
		public override void Start()
		{
			if (!this.Started)
			{
				Tracer<BaseState>.WriteInformation("Started state: {0} ({1})", new object[]
				{
					this.ToString(),
					base.MachineName
				});
				this.Started = true;
				this.Machine.MachineStarted += new EventHandler(this.MachineStarted);
				this.Machine.MachineStopped += new EventHandler(this.MachineStopped);
				this.Machine.MachineEnded += new EventHandler<TransitionEventArgs>(this.MachineEnded);
				this.Machine.MachineErrored += new EventHandler<BaseStateMachineErrorEventArgs>(this.MachineErrored);
				this.Machine.Start();
				return;
			}
			Tracer<BaseState>.WriteWarning("Trying to start state {0} which is already started!", new object[]
			{
				this.ToString()
			});
		}
		public override void Stop()
		{
			if (this.Started)
			{
				Tracer<BaseState>.WriteInformation("Stopped state: {0} ({1})", new object[]
				{
					this.ToString(),
					base.MachineName
				});
				this.Started = false;
				this.Machine.Stop();
				this.Machine.MachineStarted -= new EventHandler(this.MachineStarted);
				this.Machine.MachineStopped -= new EventHandler(this.MachineStopped);
				this.Machine.MachineEnded -= new EventHandler<TransitionEventArgs>(this.MachineEnded);
				this.Machine.MachineErrored -= new EventHandler<BaseStateMachineErrorEventArgs>(this.MachineErrored);
				return;
			}
			Tracer<BaseState>.WriteWarning("Trying to stop state {0} which is already stopped!", new object[]
			{
				this.ToString()
			});
		}
		public void ClearStateMachine()
		{
			this.Machine = new BaseStateMachine();
		}
		protected virtual void OnCurrentStateChanged(BaseState oldValue, BaseState newValue)
		{
		}
		protected virtual void MachineEnded(object sender, TransitionEventArgs args)
		{
			Tracer<StateMachineState>.WriteInformation("Machine Ended {0} ({1})", new object[]
			{
				this.ToString(),
				base.MachineName
			});
			this.RaiseStateFinished(args);
		}
		protected virtual void MachineErrored(object sender, BaseStateMachineErrorEventArgs eventArgs)
		{
			Tracer<StateMachineState>.WriteInformation("Machine Errored {0} ({1})", new object[]
			{
				this.ToString(),
				base.MachineName
			});
			this.RaiseStateErrored(eventArgs.Error);
		}
		protected virtual void MachineStopped(object sender, EventArgs args)
		{
			Tracer<StateMachineState>.WriteInformation("Machine Stopped {0} ({1})", new object[]
			{
				this.ToString(),
				base.MachineName
			});
		}
		protected virtual void MachineStarted(object sender, EventArgs args)
		{
			Tracer<StateMachineState>.WriteInformation("Machine Started {0} ({1})", new object[]
			{
				this.ToString(),
				base.MachineName
			});
			this.RaiseStateStarted(args);
		}
	}
}
