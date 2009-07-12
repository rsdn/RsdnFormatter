using System;
using System.Configuration;

namespace Rsdn.Framework.Common
{
	/// <summary>
	/// Provides access to configuration settings.
	/// This class cannot be inherited.
	/// </summary>
	public sealed class ConfigManager: RsdnMbrObject
	{
		/// <summary>
		/// Since this class provides only static methods, 
		/// make the default constructor private to prevent instances 
		/// from being created with "new ConfigManager()".
		/// </summary>
		private ConfigManager()
		{
		}

		/// <summary>
		/// Returns object from the configuration file for the given key.
		/// If string does not exist, it throws an exception.
		/// </summary>
		/// <param name="key">Name of the key.</param>
		/// <returns>Result object.</returns>
		public static object GetObject(string key)
		{
			return GetObject(key, null);
		}

		/// <summary>
		/// Returns object from the configuration file for the given key.
		/// If key does not exist and defaultValue is not null, 
		/// it return default value. Otherwise, it throws an exception.
		/// </summary>
		/// <param name="key">Name of the key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Result object.</returns>
		public static object GetObject(string key, object defaultValue)
		{
			object o = ConfigurationManager.AppSettings[key];

			// If key does not exist, we'll have null.
			// In this case we throw our own exception object.
			//
			if (o == null) 
			{
				if (defaultValue == null)
					throw new ConfigManagerException(
						string.Format("The '{0}' key is not found.", key));

				o = defaultValue;
			}

			return o;
		}

		/// <summary>
		/// Returns string from the configuration file for the given key.
		/// If string does not exist, it throws an exception.
		/// </summary>
		/// <param name="key">Name of the key.</param>
		/// <returns>Result string.</returns>
		public static string GetString(string key)
		{
			return Convert.ToString(GetObject(key, null));
		}

		/// <summary>
		/// Returns string from the configuration file for the given key.
		/// If string does not exist, it return default value.
		/// </summary>
		/// <param name="key">Name of the key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Result string.</returns>
		public static string GetString(string key, string defaultValue)
		{
			return Convert.ToString(GetObject(key, defaultValue));
		}

		/// <summary>
		/// Returns value from the configuration file for the given key.
		/// If value does not exist, it throws an exception.
		/// </summary>
		/// <param name="key">Name of the key.</param>
		/// <returns>Result value.</returns>
		public static int GetInt(string key)
		{
			return Convert.ToInt32(GetObject(key, null));
		}

		/// <summary>
		/// Returns value from the configuration file for the given key.
		/// If value does not exist, it return default value.
		/// </summary>
		/// <param name="key">Name of the key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Result value.</returns>
		public static int GetInt(string key, int defaultValue)
		{
			return Convert.ToInt32(GetObject(key, defaultValue));
		}
	}
}
