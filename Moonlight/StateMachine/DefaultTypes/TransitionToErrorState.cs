using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class TransitionToErrorState : BaseState
	{
		private readonly Exception exception;
		public TransitionToErrorState(BaseErrorState errorState, Exception exception)
		{
			this.exception = exception;
			base.AddErrorTransition(new ErrorTransition(errorState), exception);
		}
		public override void Start()
		{
			this.RaiseStateErrored(new Error(this.exception));
		}
	}
}
