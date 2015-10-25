using System;
namespace Moonlight.Common
{
	public class EventArgs<T> : EventArgs
	{
		public T Value
		{
			get;
			set;
		}
		public EventArgs()
		{
		}
		public EventArgs(T value) : this()
		{
			this.Value = value;
		}
	}
}
