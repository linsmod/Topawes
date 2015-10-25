using Moonlight.StateMachine.BaseTypes;
using System;
namespace Moonlight.StateMachine.DefaultTypes
{
	public class EndState : BaseState
	{
		public string Status
		{
			get;
			private set;
		}
		public EndState()
		{
			this.Status = string.Empty;
		}
		public EndState(string status)
		{
			this.Status = status;
		}
		public override void Start()
		{
		}
		public override void Stop()
		{
		}
	}
}
