using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.Transitions
{
	public class StateStatusTransition : BaseTransition
	{
		private readonly string statusKey;
		public StateStatusTransition(BaseState next, string statusKey) : base(next)
		{
			this.statusKey = statusKey;
		}
		public override bool ConditionsAreMet(object sender, TransitionEventArgs eventArgs)
		{
			return eventArgs.Status == this.statusKey;
		}
	}
}
