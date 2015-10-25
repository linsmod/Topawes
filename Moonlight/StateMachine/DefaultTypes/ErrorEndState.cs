using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class ErrorEndState : BaseErrorState
	{
		public override void Start(Error error)
		{
			this.RaiseStateErrored(error);
		}
		public override void Stop()
		{
		}
	}
}
