using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Инкапсулирует функции форматирования.
	/// </summary>
	public static partial class Format
	{
		private static readonly Regex _ampersandDetector =
			new Regex(
				@"&(?!#([0-9]+|x[0-9a-f]+);)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги.
		/// </summary>
		/// <param name="str">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static string ReplaceTags(this string str)
		{
			return
				string.IsNullOrEmpty(str)
					? str
					: ReplaceTagsWQ(str).Replace("\"", "&quot;");
		}

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги исключая '"'.
		/// </summary>
		/// <param name="str">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static string ReplaceTagsWQ(this string str)
		{
			return
				string.IsNullOrEmpty(str)
					? str
					: _ampersandDetector.Replace(str, "&amp;")
						.Replace(">", "&gt;")
						.Replace("<", "&lt;");
		}

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги.
		/// </summary>
		/// <param name="str">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static string ReplaceTags(this object str)
		{
			return str == null ? null : ReplaceTags(str.ToString());
		}

		/// <summary>
		/// Подготавливает текст для JScript.
		/// </summary>
		/// <warning>Текст не должен содержать спецмаркеры :quotes:, :apostroph: !</warning>
		/// <param name="str">Исходная строка.</param>
		/// <returns>Преобразованная строка.</returns>
		public static string EncodeJScriptText(this string str)
		{
			return
				string.IsNullOrEmpty(str)
				? str
				: str
					.Replace("\"", ":quotes:")
					.Replace("'", ":apostroph:")
					.Replace(@"\", @"\\")
					.Replace(":quotes:", @"\""")
					.Replace(":apostroph:", @"\'");
		}

		/// <summary>
		/// Подготавливает текст для предотовращения XSS (Cross Site Scripting)
		/// Используется, в основном для кодирования адресов (ссылок, картинок).
		/// </summary>
		/// <param name="value">Исходный текст.</param>
		/// <returns>Преобразованный текст.</returns>
		public static string EncodeAgainstXSS(this string value)
		{
			return
				string.IsNullOrEmpty(value)
					? value
					: value
						.Replace(" ", "%20")
						.Replace("\t", "%09")
						.Replace("\'", "%27");
		}

		/// <summary>
		/// Преобразует object в int. 
		/// В случае возникновения исключения возвращается 0.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <returns>Результат.</returns>
		public static int ToInt(this object o)
		{
			return ToInt(o, 0);
		}

		/// <summary>
		/// Преобразует object в int. 
		/// В случае возникновения исключения возвращается errorValue.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <param name="errorValue">Значение возвращаемое если произошла ошибка.</param>
		/// <returns>Результат.</returns>
		public static int ToInt(this object o, int errorValue)
		{
			if (o == null || string.Empty.Equals(o))
				return errorValue;

			if (o is int)
				return (int) o;

			int value;
			return int.TryParse(o.ToString(), out value) ? value : errorValue;
		}

		/// <summary>
		/// Преобразует object в double. 
		/// В случае возникновения исключения возвращается 0.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <returns>Результат.</returns>
		public static double ToDouble(this object o)
		{
			if (o == null)
				return 0;

			if (o is double)
				return (double) o;

			double value;
			return double.TryParse(o.ToString(), out value) ? value : 0;
		}


		/// <summary>
		/// Message tag extractor
		/// </summary>
		private static readonly Regex _tagsExtractor =
			new Regex(@"(?<tag>[^\s"",]+)|""(?<tag>.+?)""", RegexOptions.Compiled);

		/// <summary>
		/// Extract tags from string
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string[] ExtractTags(this string tags)
		{
			if (string.IsNullOrEmpty(tags))
				return new string[0];

			var mc = _tagsExtractor.Matches(tags);
			var exTags = new string[mc.Count];

			for (int i = 0; i < mc.Count; i++)
				exTags[i] = mc[i].Groups["tag"].ToString().ToLowerInvariant();

			return exTags;
		}

		/// <summary>
		/// Format tags
		/// </summary>
		/// <param name="tags"></param>
		/// <param name="eval">Tag transformer</param>
		/// <returns></returns>
		public static string ExtractTags(this string tags, MatchEvaluator eval)
		{
			if (string.IsNullOrEmpty(tags))
				return tags;
			return _tagsExtractor.Replace(tags, eval);
		}


		///<summary>
		/// Убирает цитирование из текста сообщения.
		///</summary>
		/// <param name="msg">Сообщение.</param>
		/// <returns>Обработанное сообщение</returns>
		public static string RemoveQuotations(string msg)
		{
			msg = TextFormatter.RemoveTaglineTag(msg);
			msg = TextFormatter.RemoveModeratorTag(msg);
			msg = Regex.Replace(msg, "Здравствуйте.*ы писали:", "");
			msg = msg.ReplaceTags();
			msg = Regex.Replace(msg, TextFormatter.StartCitation + ".*$", "", RegexOptions.Multiline);

			return msg;
		}

		/// <summary>
		/// Инкапсулирует функции форматирования даты.
		/// </summary>
		public class Date
		{
			private readonly DateTime _dateTimeValue;

			/// <summary>
			/// Контсруктор объекта.
			/// </summary>
			public Date()
			{
				_dateTimeValue = DateTime.Now;
			}

			/// <summary>
			/// Конструктор объекта.
			/// </summary>
			/// <param name="dateTime">Дата.</param>
			public Date(DateTime dateTime)
			{
				_dateTimeValue = dateTime;
			}

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
			/// Correct server time to client timezone's time
			/// </summary>
			/// <param name="serverTime">Server time</param>
			/// <returns>Corrected time</returns>
			public static DateTime CorrectToClientTime(DateTime serverTime)
			{
				return Correct(serverTime, GetClientTimeZoneOffset());
			}

			/// <summary>
			/// Correct server time to client time zone's time
			/// </summary>
			/// <param name="serverTime">Server time</param>
			/// <param name="clientTimezoneOffsetMinutes">Client time zone's offset in minutes</param>
			/// <returns></returns>
			public static DateTime Correct(DateTime serverTime, double clientTimezoneOffsetMinutes)
			{
				return serverTime.ToUniversalTime().AddMinutes(clientTimezoneOffsetMinutes);
			}

			/// <summary>
			/// Correct server time to client time zone's time
			/// </summary>
			/// <param name="serverTime">Server time. If not DateTime then return zero date</param>
			/// <param name="clientTimezoneOffsetMinutes">Client time zone's offset in minutes</param>
			/// <returns></returns>
			public static DateTime Correct(object serverTime, double clientTimezoneOffsetMinutes)
			{
				return (serverTime is DateTime)
				       	?
				       		((DateTime) serverTime).ToUniversalTime().AddMinutes(clientTimezoneOffsetMinutes)
				       	:
				       		new DateTime(0);
			}

			/// <summary>
			/// Форматирует дату используя формат "dd.MM.yy HH:mm"
			/// </summary>
			/// <returns>Результирующая строка.</returns>
			public string ToYearString()
			{
				return ToYearString(_dateTimeValue);
			}

			/// <summary>
			/// Форматирует дату используя формат "dd.MM.yy HH:mm"
			/// </summary>
			/// <param name="dateTime">Форматируемая дата.</param>
			/// <returns>Результирующая строка.</returns>
			public static string ToYearString(DateTime dateTime)
			{
				return dateTime.ToString("dd.MM.yy HH:mm");
			}

			/// <summary>
			/// Форматирует дату используя формат "dd.MM.yy"
			/// </summary>
			/// <returns>Результирующая строка.</returns>
			public string ToLongString()
			{
				return ToLongString(_dateTimeValue);
			}

			/// <summary>
			/// Форматирует дату используя формат "dd.MM.yy"
			/// </summary>
			/// <param name="dateTime">Форматируемая дата.</param>
			/// <returns>Результирующая строка.</returns>
			public static string ToLongString(DateTime dateTime)
			{
				return dateTime.ToString("dd.MM.yy");
			}

			/// <summary>
			/// Форматирует дату используя формат "dd.MM HH:mm"
			/// </summary>
			/// <returns>Результирующая строка.</returns>
			public string ToShortString()
			{
				return ToShortString(_dateTimeValue);
			}

			/// <summary>
			/// Форматирует дату используя формат "dd.MM HH:mm"
			/// </summary>
			/// <param name="dateTime">Форматируемая дата.</param>
			/// <returns>Результирующая строка.</returns>
			public static string ToShortString(DateTime dateTime)
			{
				return dateTime.ToString("dd.MM HH:mm");
			}

			/// <summary>
			/// Форматирует дату в зависимости от ее давности.
			/// Больше полугода - "dd.MM.yy.", меньше - "dd/MM HH:mm"
			/// </summary>
			/// <returns>Результирующая строка.</returns>
			public string ToDependString()
			{
				return ToDependString(_dateTimeValue);
			}

			/// <summary>
			/// Форматирует дату в зависимости от ее давности.
			/// Больше полугода - "dd.MM.yy.", меньше - "dd/MM HH:mm"
			/// </summary>
			/// <param name="dateTime">Форматируемая дата.</param>
			/// <returns>Результирующая строка.</returns>
			public static string ToDependString(DateTime dateTime)
			{
				return (dateTime > DateTime.Now.AddMonths(-6))
				       	?
				       		ToShortString(dateTime)
				       	: ToLongString(dateTime);
			}

			/// <summary>
			/// Возвращает дату на начало текущего дня.
			/// </summary>
			/// <returns>Начало текущего дня.</returns>
			public static DateTime GetDayBeginning()
			{
				return GetDayBeginning(DateTime.Now);
			}

			/// <summary>
			/// Возвращает дату на начало заданного дня.
			/// </summary>
			/// <param name="dateTime">Заданная дата.</param>
			/// <returns>Начало дня.</returns>
			public static DateTime GetDayBeginning(DateTime dateTime)
			{
				return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			}

			/// <summary>
			/// Возвращает дату на начало текущего месяца.
			/// </summary>
			/// <returns>Начало месяца.</returns>
			public static DateTime GetMonthBeginning()
			{
				return GetMonthBeginning(DateTime.Now);
			}

			/// <summary>
			/// Возвращает дату на начало заданного месяца.
			/// </summary>
			/// <param name="dateTime">Заданная дата.</param>
			/// <returns>Начало месяца.</returns>
			public static DateTime GetMonthBeginning(DateTime dateTime)
			{
				return new DateTime(dateTime.Year, dateTime.Month, 1);
			}
		}

		/// <summary>
		/// Top level RSDN domain name
		/// </summary>
		public static string RsdnDomainName = "rsdn.ru";
	}
}