using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class DefaultTransition : BaseTransition
	{
		public DefaultTransition(BaseState state) : base(state)
		{
		}
		public override bool ConditionsAreMet(object sender, TransitionEventArgs eventArgs)
		{
			return true;
		}
	}
}
