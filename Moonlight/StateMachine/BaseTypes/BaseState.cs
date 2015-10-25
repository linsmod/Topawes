using Moonlight.Common.Tracing;
using Moonlight.StateMachine.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Moonlight.StateMachine.BaseTypes
{
	public class BaseState
	{
		public event EventHandler<EventArgs> StateStarted;
		public event EventHandler<TransitionEventArgs> Finished;
		public event EventHandler<Error> Errored;
		public event EventHandler Closing;
		public DefaultTransition DefaultTransition
		{
			get;
			set;
		}
		public Collection<BaseTransition> ConditionalTransitions
		{
			get;
			private set;
		}
		public ErrorTransition DefaultErrorTransition
		{
			get;
			set;
		}
		public Dictionary<Type, ErrorTransition> ErrorTransitions
		{
			get;
			private set;
		}
		public bool Started
		{
			get;
			private set;
		}
		public string MachineName
		{
			get;
			set;
		}
		protected BaseState()
		{
			this.ConditionalTransitions = new Collection<BaseTransition>();
			this.ErrorTransitions = new Dictionary<Type, ErrorTransition>();
		}
		public static BaseState NullObject()
		{
			return new BaseState();
		}
		public void AddConditionalTransition(BaseTransition transition)
		{
			this.ConditionalTransitions.Add(transition);
		}
		public void AddErrorTransition(ErrorTransition transition, Exception exception)
		{
			this.ErrorTransitions.Add(exception.GetType(), transition);
		}
		public virtual void Start()
		{
			if (!this.Started)
			{
				Tracer<BaseState>.WriteInformation(string.Format("Started state: {0} ({1})", this.ToString(), this.MachineName), new object[0]);
				this.Started = true;
				this.RaiseStateStarted(EventArgs.Empty);
				return;
			}
			Tracer<BaseState>.WriteWarning("Trying to start state {0} ({1}) which is already started!", new object[]
			{
				this.ToString(),
				this.MachineName
			});
		}
		public virtual void Stop()
		{
			if (this.Started)
			{
				Tracer<BaseState>.WriteInformation(string.Format("Stopped state: {0} ({1})", this.ToString(), this.MachineName), new object[0]);
				this.Started = false;
				return;
			}
			Tracer<BaseState>.WriteWarning("Trying to stop state {0} ({1}) which is already stopped!", new object[]
			{
				this.ToString(),
				this.MachineName
			});
		}
		public virtual BaseState NextState(TransitionEventArgs eventArgs)
		{
			BaseState baseState = this;
			BaseTransition baseTransition = this.DefaultTransition;
			Tracer<BaseState>.WriteInformation(string.Format("Getting Next state of {0} ({1})", this.ToString(), this.MachineName), new object[0]);
			try
			{
				foreach (BaseTransition current in this.ConditionalTransitions)
				{
					if (current.ConditionsAreMet(this, eventArgs))
					{
						baseTransition = current;
						Tracer<BaseState>.WriteInformation("Conditions are met for {0}", new object[]
						{
							current.ToString()
						});
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Tracer<BaseState>.WriteError(ex, "Checking transitions is failed: unexpected error", new object[0]);
				return this.HandleTransitionException(baseState, ex);
			}
			if (baseTransition != null)
			{
				if (baseTransition == this.DefaultTransition)
				{
					Tracer<BaseState>.WriteInformation("Selecting Default transition {0}", new object[]
					{
						baseTransition.Next.ToString()
					});
				}
				Tracer<BaseState>.WriteInformation(string.Format("Next state of {0} is {1}", this.ToString(), baseTransition.Next), new object[0]);
				baseState = baseTransition.Next;
			}
			return baseState;
		}
		public void Finish(string status)
		{
			this.RaiseStateFinished(string.IsNullOrEmpty(status) ? TransitionEventArgs.Empty : new TransitionEventArgs(status));
		}
		public void Error(Exception exception)
		{
			this.RaiseStateErrored(new Error(exception));
		}
		private BaseState HandleTransitionException(BaseState state, Exception exception)
		{
			Error error = new Error(exception);
			BaseErrorState baseErrorState = this.NextErrorState(error);
			if (baseErrorState != null)
			{
				baseErrorState.Start(error);
				state = baseErrorState;
			}
			return state;
		}
		public virtual BaseErrorState NextErrorState(Error error)
		{
			Tracer<BaseState>.WriteInformation(string.Format("Getting Next Error state of {0}, error code: {1}", this.ToString(), error.Message), new object[0]);
			BaseErrorState next;
			if (this.ErrorTransitions.ContainsKey(error.ExceptionType))
			{
				next = this.ErrorTransitions[error.ExceptionType].Next;
			}
			else
			{
				if (this.DefaultErrorTransition == null)
				{
					throw new InvalidOperationException("There is no error transition for code: " + error.ExceptionType);
				}
				next = this.DefaultErrorTransition.Next;
				Tracer<BaseState>.WriteInformation("Selecting Default error state {0}", new object[]
				{
					next.ToString()
				});
			}
			if (next != null)
			{
				Tracer<BaseState>.WriteInformation(string.Format("Next Error state of {0} is {1}", this.ToString(), next), new object[0]);
			}
			return next;
		}
		protected virtual void RaiseStateStarted(EventArgs eventArgs)
		{
			EventHandler<EventArgs> stateStarted = this.StateStarted;
			if (stateStarted != null)
			{
				stateStarted(this, eventArgs);
			}
		}
		protected virtual void RaiseStateFinished(TransitionEventArgs eventArgs)
		{
			EventHandler<TransitionEventArgs> finished = this.Finished;
			if (finished != null)
			{
				finished(this, eventArgs);
				return;
			}
			Tracer<BaseState>.WriteWarning("Finishing state without handler {0} ({1}), status: {2}", new object[]
			{
				this.ToString(),
				this.MachineName,
				(eventArgs == null || string.IsNullOrEmpty(eventArgs.Status)) ? "empty" : eventArgs.Status
			});
		}
		protected virtual void RaiseStateErrored(Error error)
		{
			EventHandler<Error> errored = this.Errored;
			if (errored != null)
			{
				errored(this, error);
				return;
			}
			Tracer<BaseState>.WriteWarning("Error in state without handler {0} ({1})", new object[]
			{
				this.ToString(),
				this.MachineName
			});
		}
		public override string ToString()
		{
			return base.ToString().Substring(base.ToString().LastIndexOf('.') + 1);
		}
		protected void SendClosingEvent()
		{
			EventHandler closing = this.Closing;
			if (closing != null)
			{
				closing(this, EventArgs.Empty);
			}
		}
	}
}
