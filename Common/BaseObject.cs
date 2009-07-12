using System;
using System.Runtime.Serialization;

namespace Rsdn.Framework.Common
{
	/// <summary>
	/// Common RSDN interface.
	/// </summary>
	public interface IRsdnObject
	{
	}


	/// <summary>
	/// The base class for simple objects.
	/// </summary>
	public class RsdnObject : IRsdnObject
	{
	}


	/// <summary>
	/// The base class for simple disposable objects.
	/// </summary>
	public class RsdnDisposableObject : DisposableObject, IRsdnObject
	{
	}


	/// <summary>
	/// The base class for MBR objects.
	/// </summary>
	public class RsdnMbrObject : MarshalByRefObject, IRsdnObject
	{
	}


	/// <summary>
	/// The base class for MBR disposable objects.
	/// </summary>
	public class RsdnDisposableMbrObject : DisposableMbrObject, IRsdnObject
	{
	}


	/// <summary>
	/// Defines the base class for exceptions.
	/// </summary>
	[Serializable] 
	public class RsdnException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the RsdnException class.
		/// </summary>
		public RsdnException() 
            : base("RSDN Exception")
		{
		}
    
    /// <summary>
    /// RSDN specific error codes.
    /// </summary>
		public enum ErrorCodes
    {
    	NoSuchMessage = 5,
			NoSuchGroup = 6,
			NoSuchLinkType = 7,
			NoSpace = 8,
			FileAlreadyExists = 9,
			BadLogin = 10
		};

		/// <summary>
		/// Initializes a new instance of the RsdnException class 
		/// with the specified error code and message.
		/// </summary>
		/// <param name="errorCode">Code of the error.</param>
		/// <param name="message">The message to display to the client when the exception is thrown.</param>
		public RsdnException(ErrorCodes errorCode, string message) 
			: base(message) 
		{
			ErrorCode = (int)errorCode;
		}

		/// <summary>
		/// Initializes a new instance of the RsdnException class 
		/// with the specified error code, message and inner exception.
		/// </summary>
		/// <param name="errorCode">Code of the error.</param>
		/// <param name="message">The message to display to the client when the exception is thrown.</param>
		/// <param name="exception">The InnerException, if any, that threw the current exception.</param>
		public RsdnException(ErrorCodes errorCode, string message, Exception exception)
			: base(message, exception)
		{
			ErrorCode = (int)errorCode;
		}

		/// <summary>
		/// Initializes a new instance of the RsdnException class 
		/// with the specified error code and message.
		/// </summary>
		/// <param name="message">The message to display to the client when the exception is thrown.</param>
		/// <param name="errorCode">Code of the error.</param>
		public RsdnException(int errorCode, string message) 
			: base(message) 
		{
			ErrorCode = errorCode;
		}
    	
		/// <summary>
		/// Initializes a new instance of the RsdnException class 
		/// with the specified error message.
		/// </summary>
		/// <param name="message">The message to display to the client when the exception is thrown.</param>
		public RsdnException(string message) 
			: base(message) 
		{
		}
    	
		/// <summary>
		/// Initializes a new instance of the RsdnException class
		/// with the specified error message and InnerException property.
		/// </summary>
		/// <param name="message">The message to display to the client when the exception is thrown.</param>
		/// <param name="innerException">The InnerException, if any, that threw the current exception.</param>
		public RsdnException(string message, Exception innerException) 
			: base(message, innerException) 
		{
		}

		private int errorCodeValue;
		/// <summary>
		/// The error code.
		/// </summary>
		public int ErrorCode
		{
			get { return errorCodeValue;  }
			set { errorCodeValue = value; }
		}

		/// <summary>
		/// The error code of <see cref="ErrorCodes"/>.
		/// </summary>
		public ErrorCodes ErrorCodeEx
		{
			get { return (ErrorCodes)errorCodeValue; }
			set { errorCodeValue = (int)value; }
		}

		/// <summary>
		/// Initializes a new instance of the SupraException class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <remarks>This constructor is called during deserialization to reconstitute the exception object transmitted over a stream.</remarks>
		protected RsdnException(SerializationInfo info,StreamingContext context) 
			: base(info,context) 
		{
		}
	}
}
