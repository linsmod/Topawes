using System;
namespace Moonlight.StateMachine.BaseTypes
{
	public class BaseStateMachineErrorEventArgs : EventArgs
	{
		public Error Error
		{
			get;
			private set;
		}
		public BaseStateMachineErrorEventArgs(Error error)
		{
			this.Error = error;
		}
	}
}
