/*
 * File:    ConfigManagerException.cs
 * Created: 14.01.2003
 * Author:  Igor Tkachev
 *          mailto:it@rsdn.ru, mailto:igor@tkachev.com
 * 
 * Description: 
 * 
 *     Defines configuration manager exception class.
 * 
 * Changed by:
 * 
 * 1.
 *
 */

using System;

namespace Rsdn.Framework.Common
{
	/// <summary>
	/// Configuration manager exception class.
	/// </summary>
	public class ConfigManagerException : RsdnException
	{
		/// <summary>
		/// Initializes a new instance of the ConfigManagerException class.
		/// </summary>
		public ConfigManagerException() 
			: base("Configuration manager exception.")
		{
		}
        
		/// <summary>
		/// Initializes a new instance of the ConfigManagerException class 
		/// with the specified error message.
		/// </summary>
		/// <param name="message">The message to display to the client when the exception is thrown.</param>
		public ConfigManagerException(string message) 
			: base(message) 
		{
		}

    	/// <summary>
		/// Initializes a new instance of the ConfigManagerException class 
		/// with the specified error message and InnerException property.
    	/// </summary>
    	/// <param name="message">The message to display to the client when the exception is thrown.</param>
    	/// <param name="innerException">The InnerException, if any, that threw the current exception.</param>
		public ConfigManagerException(string message, Exception innerException) 
			: base(message, innerException) 
		{
		}
	}
}
