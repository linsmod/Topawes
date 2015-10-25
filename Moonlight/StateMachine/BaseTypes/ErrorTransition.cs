using System;
namespace Moonlight.StateMachine.BaseTypes
{
	public class ErrorTransition
	{
		public virtual BaseErrorState Next
		{
			get;
			private set;
		}
		public ErrorTransition(BaseErrorState next)
		{
			this.Next = next;
		}
	}
}
