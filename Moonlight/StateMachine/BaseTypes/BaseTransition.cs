using System;
namespace Moonlight.StateMachine.BaseTypes
{
	public abstract class BaseTransition
	{
		public virtual BaseState Next
		{
			get;
			protected set;
		}
		protected BaseTransition(BaseState next)
		{
			this.Next = next;
		}
		public override string ToString()
		{
			string text = base.ToString();
			return text.Substring(text.LastIndexOf('.') + 1);
		}
		public abstract bool ConditionsAreMet(object sender, TransitionEventArgs eventArgs);
	}
}
