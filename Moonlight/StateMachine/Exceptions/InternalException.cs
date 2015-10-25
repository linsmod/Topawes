using System;
using System.Runtime.Serialization;
namespace Moonlight.StateMachine.Exceptions
{
	[Serializable]
	public class InternalException : Exception
	{
		public InternalException()
		{
		}
		public InternalException(string message) : base(message)
		{
		}
		public InternalException(string message, Exception internalException) : base(message, internalException)
		{
		}
		protected InternalException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
