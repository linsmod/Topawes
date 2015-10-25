using Moonlight.Common.Tracing;
using System;
namespace Moonlight.StateMachine.BaseTypes
{
	public class BaseErrorState : BaseState
	{
		public new Error Error
		{
			get;
			private set;
		}
		public sealed override void Start()
		{
			base.Start();
		}
		public virtual void Start(Error error)
		{
			this.Error = error;
			Tracer<BaseErrorState>.WriteInformation("Started Error state for error: " + error.Message, new object[0]);
			base.Start();
		}
	}
}
