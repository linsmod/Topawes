using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class StartState : BaseState
	{
		public override void Start()
		{
			this.RaiseStateFinished(TransitionEventArgs.Empty);
		}
		public override void Stop()
		{
		}
	}
}
