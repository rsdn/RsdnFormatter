using System;
using System.Web;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Инкапсулирует функции форматирования.
	/// </summary>
	public static partial class Format
	{
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
		public static string RsdnDomainName = "rsdn.org";
	}
}