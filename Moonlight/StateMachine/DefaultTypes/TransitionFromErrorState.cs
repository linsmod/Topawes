using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class TransitionFromErrorState : BaseErrorState
	{
		public TransitionFromErrorState(BaseState state)
		{
			base.DefaultTransition = new DefaultTransition(state);
		}
		public override void Start(Error error)
		{
			this.RaiseStateFinished(TransitionEventArgs.Empty);
		}
	}
}
