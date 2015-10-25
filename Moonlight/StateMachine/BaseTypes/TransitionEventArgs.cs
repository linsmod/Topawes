using System;
namespace Moonlight.StateMachine.BaseTypes
{
	public class TransitionEventArgs : EventArgs
	{
		public new static TransitionEventArgs Empty
		{
			get
			{
				return new TransitionEventArgs(string.Empty);
			}
		}
		public string Status
		{
			get;
			private set;
		}
		public TransitionEventArgs(string status)
		{
			this.Status = status;
		}
	}
}
