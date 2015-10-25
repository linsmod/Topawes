using Moonlight.StateMachine.BaseTypes;
using Moonlight.StateMachine.DefaultTypes;
using System;
namespace Moonlight.StateMachine.Transitions
{
	public class PropagateErrorTransition : ErrorTransition
	{
		public PropagateErrorTransition() : base(new ErrorEndState())
		{
		}
	}
}
