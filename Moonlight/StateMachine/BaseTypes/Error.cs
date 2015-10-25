using Moonlight.Common;
using System;
namespace Moonlight.StateMachine.BaseTypes
{
	public class Error : EventArgs<Exception>
	{
		public Type ExceptionType
		{
			get
			{
				return base.Value.GetType();
			}
		}
		public string Message
		{
			get;
			set;
		}
		public Error(Exception ex) : base(ex)
		{
		}
	}
}
