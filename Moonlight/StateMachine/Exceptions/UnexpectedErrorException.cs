using System;
using System.Runtime.Serialization;
namespace Moonlight.StateMachine.Exceptions
{
	[Serializable]
	public class UnexpectedErrorException : Exception
	{
		public UnexpectedErrorException()
		{
		}
		public UnexpectedErrorException(string message) : base(message)
		{
		}
		public UnexpectedErrorException(string message, Exception internalException) : base(message, internalException)
		{
		}
		protected UnexpectedErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
