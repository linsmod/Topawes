using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class LambdaTransition : BaseTransition
	{
		private readonly Func<bool> predicate;
		public LambdaTransition(Func<bool> predicate, BaseState state) : base(state)
		{
			this.predicate = predicate;
		}
		public override bool ConditionsAreMet(object sender, TransitionEventArgs eventArgs)
		{
			return this.predicate();
		}
	}
}
