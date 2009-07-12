using System;

namespace Rsdn.Framework.Common
{
	public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
	{
		public object GetFormat(Type formatType)
		{
			return typeof(ICustomFormatter).IsAssignableFrom(formatType) ? this : null;
		}

		private const string fileSizeFormat = "fs";

		private readonly string[] letters = new[] { "B", "KB", "MB", "GB", "TB", "PB" };

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format) || !format.StartsWith(fileSizeFormat, StringComparison.Ordinal))
			{
				return defaultFormat(format, arg, formatProvider);
			}

			Decimal size;
			try
			{
				size = Convert.ToDecimal(arg);
			}
			catch (InvalidCastException)
			{
				return defaultFormat(format, arg, formatProvider);
			}

			byte i = 0;
			while ((size >= 1024) && (i < letters.Length - 1))
			{
				i++;
				size /= 1024;
			}

			var precision = format.Substring(2);
			if (String.IsNullOrEmpty(precision)) precision = "2";

			return String.Format("{0:N" + precision + "}{1}", size, letters[i]);

		}

		private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
		{
			var formattableArg = arg as IFormattable;
			return (formattableArg != null) ?
				formattableArg.ToString(format, formatProvider) : arg.ToString();
		}
	}
}
