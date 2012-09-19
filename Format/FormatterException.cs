using System;
using System.Runtime.Serialization;

namespace Rsdn.Framework.Formatting
{
	///<summary>
	/// Formatter exceptions class.
	///</summary>
	[Serializable]
	public class FormatterException : ApplicationException
	{
		/// <summary>
		/// Initialize instance.
		/// </summary>
		public FormatterException()
		{}

		/// <summary>
		/// Initialize instance with supplied message;
		/// </summary>
		public FormatterException(string message) : base(message)
		{}

		/// <summary>
		/// Initialize instance with supplied message and inner exception.
		/// </summary>
		public FormatterException(string message, Exception innerException)
			: base(message, innerException)
		{}

		/// <summary>
		/// Deserialization constructor.
		/// </summary>
		protected FormatterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}
}