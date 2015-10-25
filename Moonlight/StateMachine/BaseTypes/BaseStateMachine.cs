using Moonlight.Common.Tracing;
using Moonlight.StateMachine.DefaultTypes;
using Moonlight.StateMachine.Exceptions;
using System;
using System.Collections.Generic;
namespace Moonlight.StateMachine.BaseTypes
{
	public class BaseStateMachine
	{
		private enum StateMachineState
		{
			Running,
			Stopped
		}
		private readonly object sync = new object();
		private readonly List<BaseState> states;
		private BaseStateMachine.StateMachineState machineState;
		private BaseState currentState;
		private string machineName;
		public event EventHandler MachineStarted;
		public event EventHandler<TransitionEventArgs> MachineEnded;
		public event EventHandler MachineStopped;
		public event EventHandler<BaseStateMachineErrorEventArgs> MachineErrored;
		public event Action<BaseState, BaseState> CurrentStateChanged;
		public string MachineName
		{
			get
			{
				return this.machineName;
			}
			set
			{
				this.machineName = value.Substring(value.LastIndexOf('.') + 1);
			}
		}
		public BaseState CurrentState
		{
			get
			{
				return this.currentState;
			}
			private set
			{
				BaseState previousState = this.currentState;
				this.currentState = value;
				this.RaiseCurrentStateChanged(previousState, this.currentState);
			}
		}
		public BaseStateMachine()
		{
			this.CurrentState = BaseState.NullObject();
			this.states = new List<BaseState>();
			this.machineState = BaseStateMachine.StateMachineState.Stopped;
		}
		public void AddState(BaseState state)
		{
			if (!this.CheckIsRunning())
			{
				state.MachineName = this.MachineName;
				this.states.Add(state);
			}
		}
		public void SetStartState(BaseState state)
		{
			this.states.Remove(state);
			this.states.Insert(0, state);
		}
		public virtual void Start()
		{
			lock (this.sync)
			{
				if (!this.CheckIsRunning())
				{
					this.RaiseStateMachineEvent(this.MachineStarted);
					this.StartStateMachine();
				}
			}
		}
		public virtual void Stop()
		{
			lock (this.sync)
			{
				if (this.CheckIsRunning())
				{
					this.StopStateMachine();
					this.RaiseStateMachineEvent(this.MachineStopped);
				}
			}
		}
		public virtual void NextState()
		{
			this.NextState(TransitionEventArgs.Empty);
		}
		public bool CheckIsRunning()
		{
			return this.machineState == BaseStateMachine.StateMachineState.Running;
		}
		private void RaiseCurrentStateChanged(BaseState previousState, BaseState newState)
		{
			Action<BaseState, BaseState> currentStateChanged = this.CurrentStateChanged;
			if (currentStateChanged != null)
			{
				currentStateChanged(previousState, newState);
			}
		}
		private void NextState(TransitionEventArgs eventArgs)
		{
			lock (this.sync)
			{
				if (this.CheckIsRunning())
				{
					this.SetNextState(eventArgs);
				}
			}
		}
		private void ErroredState(Error error)
		{
			lock (this.sync)
			{
				if (this.CheckIsRunning())
				{
					this.SetErrorState(error);
				}
			}
		}
		private void StartStateMachine()
		{
			if (this.states.Count == 0)
			{
				UnexpectedErrorException ex = new UnexpectedErrorException("No states to run state machine!");
				Tracer<BaseStateMachine>.WriteError(ex);
				throw ex;
			}
			this.machineState = BaseStateMachine.StateMachineState.Running;
			this.CurrentState = this.states[0];
			this.CurrentState.Finished += new EventHandler<TransitionEventArgs>(this.CurrentStateFinished);
			this.CurrentState.Errored += new EventHandler<Error>(this.CurrentStateErrored);
			this.CurrentState.Start();
		}
		private string StopStateMachine()
		{
			this.machineState = BaseStateMachine.StateMachineState.Stopped;
			string result = string.Empty;
			EndState endState = this.CurrentState as EndState;
			if (endState != null)
			{
				result = endState.Status;
			}
			this.CurrentState.Finished -= new EventHandler<TransitionEventArgs>(this.CurrentStateFinished);
			this.CurrentState.Errored -= new EventHandler<Error>(this.CurrentStateErrored);
			this.CurrentState.Stop();
			this.CurrentState = BaseState.NullObject();
			return result;
		}
		private void SetNextState(TransitionEventArgs eventArgs)
		{
			if (!this.CheckIsRunning())
			{
				return;
			}
			BaseState baseState = this.CurrentState.NextState(eventArgs);
			this.CurrentState.Stop();
			this.CurrentState.Finished -= new EventHandler<TransitionEventArgs>(this.CurrentStateFinished);
			this.CurrentState.Errored -= new EventHandler<Error>(this.CurrentStateErrored);
			this.CurrentState = baseState;
			this.CurrentState.Finished += new EventHandler<TransitionEventArgs>(this.CurrentStateFinished);
			this.CurrentState.Errored += new EventHandler<Error>(this.CurrentStateErrored);
			try
			{
				this.CurrentState.Start();
			}
			catch (OutOfMemoryException ex)
			{
				Tracer<BaseStateMachine>.WriteError(ex, "Cannot start a state", new object[0]);
				this.CurrentStateErrored(this.CurrentState, new Error(ex));
			}
			catch (Exception ex2)
			{
				Tracer<BaseStateMachine>.WriteError(ex2, "Cannot start a state", new object[0]);
				this.CurrentStateErrored(this.CurrentState, new Error(new InternalException(string.Empty, ex2)));
			}
			if (this.IsCurrentStateEndState())
			{
				string status = this.StopStateMachine();
				EventHandler<TransitionEventArgs> machineEnded = this.MachineEnded;
				if (machineEnded != null)
				{
					machineEnded(this, new TransitionEventArgs(status));
				}
			}
		}
		private void SetErrorState(Error error)
		{
			if (!this.CheckIsRunning())
			{
				return;
			}
			if (this.IsCurrentStateEndErrorState())
			{
				this.StopStateMachine();
				this.RaiseStateMachineErroredEvent(error);
				return;
			}
			BaseErrorState baseErrorState = this.CurrentState.NextErrorState(error);
			if (baseErrorState != this.CurrentState)
			{
				this.CurrentState.Stop();
				this.CurrentState.Finished -= new EventHandler<TransitionEventArgs>(this.CurrentStateFinished);
				this.CurrentState.Errored -= new EventHandler<Error>(this.CurrentStateErrored);
				this.CurrentState = baseErrorState;
				baseErrorState.Finished += new EventHandler<TransitionEventArgs>(this.CurrentStateFinished);
				baseErrorState.Errored += new EventHandler<Error>(this.CurrentStateErrored);
				baseErrorState.Start(error);
			}
		}
		private void CurrentStateFinished(object sender, TransitionEventArgs eventArgs)
		{
			BaseState baseState = sender as BaseState;
			lock (this.sync)
			{
				if (this.CurrentState == baseState)
				{
					this.NextState(eventArgs);
				}
				else
				{
					Tracer<BaseStateMachine>.WriteWarning(string.Concat(new object[]
					{
						"Blocked state change attempt from:",
						this.CurrentState,
						" to:",
						baseState
					}), new object[0]);
				}
			}
		}
		private void CurrentStateErrored(object sender, Error error)
		{
			BaseState baseState = sender as BaseState;
			lock (this.sync)
			{
				if (this.CurrentState == baseState)
				{
					this.ErroredState(error);
				}
				else
				{
					Tracer<BaseStateMachine>.WriteWarning(string.Concat(new object[]
					{
						"Blocked state change attempt from:",
						this.CurrentState,
						" to:",
						baseState
					}), new object[0]);
				}
			}
		}
		private bool IsCurrentStateEndState()
		{
			return this.CurrentState.DefaultTransition == null && this.CurrentState.ConditionalTransitions.Count == 0;
		}
		private bool IsCurrentStateEndErrorState()
		{
			return this.CurrentState is ErrorEndState;
		}
		private void RaiseStateMachineEvent(EventHandler eventHandler)
		{
			if (eventHandler != null)
			{
				eventHandler(this, EventArgs.Empty);
			}
		}
		private void RaiseStateMachineErroredEvent(Error error)
		{
			EventHandler<BaseStateMachineErrorEventArgs> machineErrored = this.MachineErrored;
			if (machineErrored != null)
			{
				machineErrored(this, new BaseStateMachineErrorEventArgs(error));
			}
		}
	}
}
