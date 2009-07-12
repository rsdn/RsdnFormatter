using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Rsdn.Framework.Common
{
	/// <summary>
	/// Всякая полезная мелочь.
	/// </summary>
	public sealed class Utils: RsdnMbrObject
	{
		/// <summary>
		/// Возвращает индекс, который можно использовать для построения окончаний
		/// слов в зависимости от значения числа.
		/// </summary>
		/// <remarks>
		/// Возвращаемый индекс соответствует следующим значениям:<br/>
		/// <list type="bullet">
		/// <item>0 - ноль рублей</item>
		/// <item>1 - один рубль</item>
		/// <item>2,3 - два рубля</item>
		/// </list>
		/// </remarks>
		/// <example>
		/// Следующий пример демонстрирует применение функции.
		/// <code>
		/// void Main()
		/// {
		///     string[] money = 
		///     {
		///         "денёг",
		///         "деньга",
		///         "денюжки"
		///     };
		/// 
		///     System.Console.WriteLine("1 {0}",money[HowSay(1)]);
		///     System.Console.WriteLine("104 {0}",money[HowSay(104)]);
		///     System.Console.WriteLine("58 {0}",money[HowSay(58)]);
		/// }
		/// </code>
		/// </example>
		/// <param name="n">Целое число</param>
		/// <returns>Индекс.</returns>
		public static int HowSay(int n)
		{
			// Всё происходит в пределах сотни.
			//
			n %= 100;

			// Между 10 и 20 разницы нет.
			//
			if (n >= 10  &&  n <= 20) 
				return 0;

			// Всё происходит даже в пределах десятки.
			//
			switch (n % 10)
			{
				case 1: return 1;
				case 2:
				case 3: return 2;
			}
			return 0;
		}

		/// <summary>
		/// Преобразует строку, содержащую escape-последовательности, к нормальному виду.
		/// </summary>
		/// <param name="str">Исходная строка.</param>
		/// <returns>Результирующая строка</returns>
		public static string Unescape(string str)
		{
			if (string.IsNullOrEmpty(str))
				return string.Empty;

			var sb = new StringBuilder(str.Length);
			var current = 0;
			int next;

			sb.Length = 0;
			do 
			{
				var index = str.IndexOf('%', current);

				if (str.Length - index < 3) 
					index = -1;

				next = index == -1? str.Length: index;

				sb.Append(str.Substring(current,next - current));
				if (index != -1) 
				{
					var bytes = new byte[str.Length - index];
					var byteCount = 0;
					char ch;

					do 
					{
						ch = Uri.HexUnescape(str,ref next);
						if (ch < '\x80') 
							break;

						bytes[byteCount++] = (byte)ch;
					} while (next < str.Length);
					
					if (byteCount != 0) 
					{
						var charCount = Encoding.UTF8.GetCharCount(bytes,0,byteCount);

						if (charCount != 0) 
						{
							var chars = new char[str.Length - index];
							Encoding.UTF8.GetChars(bytes,0,byteCount,chars,0);
							sb.Append(chars,0,charCount);
						} 
						else 
						{
							for (var i=0; i<byteCount; ++i) 
								sb.Append((char)bytes[i]);
						}
					}

					if (ch < '\x80') 
					{
						sb.Append(ch);
					}
				}

				current = next;
			} while (next < str.Length);

			return sb.ToString();
		}

		
		/// <summary>
		/// Regular expression for detecting invalid XML characters.
		/// </summary>
		public static Regex InvalidXmlCharacters = new Regex(@"[^\x09\x0A\x0D\u0020-\uD7FF\uE000-\uFFFD]");

		/// <summary>
		/// Remove invalid xml characters.
		/// </summary>
		/// <param name="text">Input text.</param>
		/// <param name="replacingSymbol">Replacing symbol for invalid characters.</param>
		/// <returns>Processed text.</returns>
		public static string ProcessInvalidXmlCharacters(string text, string replacingSymbol)
		{
			return string.IsNullOrEmpty(text) ? text :
				InvalidXmlCharacters.Replace(text, replacingSymbol);
		}

		/// <summary>
		/// Remove invalid xml characters (replace to "□").
		/// </summary>
		/// <param name="text">Input text.</param>
		/// <returns>Processed text.</returns>
		public static string ProcessInvalidXmlCharacters(string text)
		{
			return ProcessInvalidXmlCharacters(text, "□");
		}

		/// <summary>
		/// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
		/// </summary>
		/// <param name="number">Numeric value to be converted.</param>
		/// <returns>Converted text value of numeric value.</returns>
		[Obsolete("Use Rsdn.Framework.Common.FileSizeFormatProvider instead.")]
		public static string BytesToString(long number)
		{
			return string.Format(new FileSizeFormatProvider(), "{0:fs}", number);
		}

		/// <summary>
		/// Get hash of password salted by username.
		/// </summary>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		/// <returns>Hash of password</returns>
		public static string GetPasswordHash(string username, string password)
		{
			// due some bug in .NET Framework - create provider each time
			using (var md5Provider =
							 new MD5CryptoServiceProvider())
			{
				var result = Convert.ToBase64String(
					md5Provider.ComputeHash(Encoding.UTF8.GetBytes(((username != null) ? username.ToLower() : null) + password)));
				// 50 is max length of password field in database.
				return result.Substring(0, Math.Min(result.Length, 50));
			}
		}

		public static string ResolvePath(string path)
		{
			return string.IsNullOrEmpty(path) ? path :
				path.Replace("~", ApplicationRoot);
		}

		public static string ApplicationRoot
		{
			get
			{
				return (HttpContext.Current == null ||
					HttpContext.Current.Request.ApplicationPath == "/") ?
					"" : HttpContext.Current.Request.ApplicationPath;
			}
		}
	}
}
