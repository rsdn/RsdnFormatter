using System;
using System.Web;
using JetBrains.Annotations;
using Rsdn.Framework.Formatting;

namespace Rsdn.Formatting.OldWeb
{
	[PublicAPI]
	public class DateHelpers
	{
		/// <summary>
		/// Get client time zone's offset in minutes from HttpContext
		/// </summary>
		/// <remarks>If HttpContext is absent, no offset is provided</remarks>
		/// <returns></returns>
		public static double GetClientTimeZoneOffset()
		{
			double timezoneOffsetMinutes = 0;
			if (HttpContext.Current != null)
			{
				var tzCookie = HttpContext.Current.Request.Cookies["tz"];
				double val;
				timezoneOffsetMinutes = 
					tzCookie != null && double.TryParse(tzCookie.Value, out val)
						? val
						: 0;
			}
			return timezoneOffsetMinutes;
		}

		/// <summary>
		/// Correct client time to server time.
		/// </summary>
		/// <param name="clientTime">Client time.</param>
		/// <returns>Server time.</returns>
		public static DateTime CorrectToServerTime(DateTime clientTime)
		{
			return clientTime.AddMinutes(-GetClientTimeZoneOffset()).ToLocalTime();
		}

		/// <summary>
		/// Correct server time to client timezone time
		/// </summary>
		/// <param name="serverTime">Server time</param>
		/// <returns>Corrected time</returns>
		public static DateTime CorrectToClientTime(DateTime serverTime)
		{
			return Format.Date.Correct(serverTime, GetClientTimeZoneOffset());
		}
	}
}
