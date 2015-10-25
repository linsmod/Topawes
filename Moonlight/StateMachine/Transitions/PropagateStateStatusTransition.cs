using Moonlight.StateMachine.BaseTypes;
using Moonlight.StateMachine.DefaultTypes;
using System;
namespace Moonlight.StateMachine.Transitions
{
	public class PropagateStateStatusTransition : BaseTransition
	{
		private readonly string statusKey;
		public PropagateStateStatusTransition(string statusKey) : base(new EndState(statusKey))
		{
			this.statusKey = statusKey;
		}
		public override bool ConditionsAreMet(object sender, TransitionEventArgs eventArgs)
		{
			return eventArgs.Status == this.statusKey;
		}
	}
}
