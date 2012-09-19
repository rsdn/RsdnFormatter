using System;
using System.Text.RegularExpressions;

namespace Rsdn.Framework.Formatting
{
	public static partial class Format
	{
		/// <summary>
		/// Инкапсулирует функции форматирования сообщений форума.
		/// </summary>
		public class Forum
		{
			/// <summary>
			/// Regex for detecting Re: and Re[number]: prefixes in subject at the start of the line
			/// </summary>
			private static readonly Regex _reDetector =
				new Regex(@"(?in)^((Re|На)(\[(?<number>\d+)\])?:\s*)+", RegexOptions.Compiled);

			/// <summary>
			/// Готовит поле 'тема' для редактирования.
			/// </summary>
			/// <param name="subj">Название темы.</param>
			/// <returns>Результат.</returns>
			public static string GetEditSubject(string subj)
			{
				if (string.IsNullOrEmpty(subj))
					return subj;

				// Удаляем элемент Re[..]
				return _reDetector.Replace(subj, "");
			}

			/// <summary>
			/// Корректировка темы сообщения.
			/// </summary>
			/// <remarks>
			/// Добавляет префикс '<b>Re</b>' к новой теме сообщения.
			/// Для первого ответа в ветке добавляется префикс '<b>Re:</b>', 
			/// для всех последующий '<b>Re[n]:</b>', где <b>n</b> - уровень вложенности.
			/// </remarks>
			/// <param name="oldSubject">Тема предыдущего сообщения.</param>
			/// <param name="newSubject">Тема нового сообщения.</param>
			/// <returns>Возвращает сообщение с добавленным префиксом 'Re'.</returns>
			public static string AdjustSubject(string oldSubject, string newSubject)
			{
				return GetRePrefix(GetSubjectDeep(oldSubject) + 1) + newSubject;
			}

			/// <summary>
			/// Возвращает уровень вложенности темы сообщения.
			/// </summary>
			/// <param name="subject">Тема сообщения</param>
			/// <returns>Уровень вложенности.</returns>
			public static int GetSubjectDeep(string subject)
			{
				var level = 0;
				var reMatch = _reDetector.Match(subject);
				if (reMatch.Success)
				{
					level =
						!reMatch.Groups["number"].Success
						? 1
						: reMatch.Groups["number"].Captures[0].Value.ToInt();
				}
				return level;
			}

			/// <summary>
			/// Возвращает префикс темы сообщения.
			/// </summary>
			/// <remarks>
			/// Для первого ответа в ветке префикс '<b>Re:</b>', 
			/// для всех последующий '<b>Re[n]:</b>', где <b>n</b> - уровень вложенности.
			/// </remarks>
			/// <param name="level">Уровень вложенности сообщения.</param>
			/// <returns>Префикс.</returns>
			private static string GetRePrefix(int level)
			{
				return (level == 1) ? "Re: " : string.Format("Re[{0}]: ", level);
			}

			/// <summary>
			/// Корректировка темы сообщения.
			/// </summary>
			/// <remarks>
			/// Корректирует уровень вложенности сообщения (Re[xxx])
			/// относительно нового корня.
			/// </remarks>
			/// <param name="level">Уровень вложенности корневого сообщения.</param>
			/// <param name="subject">Тема сообщения.</param>
			/// <returns>Откорректированная тема сообщения.</returns>
			public static string AdjustSubject(int level, string subject)
			{
				return _reDetector.Replace(subject, match =>
					GetRePrefix(match.Groups["number"].Success ? int.Parse(match.Groups["number"].Captures[0].Value) - level : 1));
			}

			/// <summary>
			/// Возвращает сокращения для заданного ника.
			/// </summary>
			/// <param name="nick">Ник.</param>
			/// <returns>Сокращение.</returns>
			public static string GetShortNick(string nick)
			{
				// Получаем сокращения для ника.
				//
				var shortname = "";

				if (nick.Length <= 3 && !nick.Contains(" "))
				{
					// Ник короче трёх символов.
					shortname = nick
						.Replace("&", "")
						.Replace("<", "")
						.Replace(">", "")
						.Replace("\"", "")
						.Replace("'", "");
				}
				else if (nick == "Igor Trofimov")
				{
					shortname = "iT";
				}
				else if (nick == "_MarlboroMan_")
				{
					shortname = "_MM_";
				}
				else if (nick == "Hacker_Delphi")
				{
					shortname = "H_D";
				}
				else
				{
					// Заменяем все левые символы на пробелы.
					var un = Regex.Replace(nick, @"\W+", " ").Trim();

					// Удаляем символы нижнего регистра и цифры.
					if (!un.Contains(" "))
					{
						shortname = Regex.Replace(un, @"[a-zа-я0-9]", "");
						if (shortname.Length > 3)
							shortname = shortname.Substring(0, 3);
					}

					// Если ник весь в нижнем регистре.
					if (shortname.Length == 0)
					{
						// Разваливаем на массив слов.
						var sa = un.Split(' ');

						// Берём только первые символы.
						for (var i = 0; i < sa.Length && i < 3; i++)
							if (sa[i].Length > 0)
								shortname += sa[i][0];

						// Приводим к верхнему регистру.
						shortname = shortname.ToUpper();
					}
				}

				return shortname;
			}

			/// <summary>
			/// Готовит сообщение к цитированию.
			/// </summary>
			/// <param name="msg">Сообщение.</param>
			/// <param name="nick">Автор ссобщения.</param>
			/// <returns>Обработанное сообщение</returns>
			public static string GetEditMessage(string msg, string nick)
			{
				return GetEditMessage(msg, nick, false);
			}

			/// <summary>
			/// Готовит сообщение к цитированию.
			/// </summary>
			/// <param name="msg">Сообщение.</param>
			/// <param name="nick">Автор ссобщения.</param>
			/// <param name="moderator">== true, если пользователь - модератор.</param>
			/// <returns>Обработанное сообщение</returns>
			public static string GetEditMessage(string msg, string nick, bool moderator)
			{
				// Получаем сокращения для ника.
				//
				var shortname = GetShortNick(nick);

				// Если пользователь не модератор. тег [moderator] удаляется.
				// Дополнительная проверка производиться при сохранении сообщения.
				//
				if (moderator == false)
				{
					msg = TextFormatter.RemoveModeratorTag(msg);
				}
				// Убираем таглайн
				//
				msg = TextFormatter.RemoveTaglineTag(msg);

				// Формирование цитаты.
				//
				msg = Regex.Replace(msg, @"^\s*[-\w\.]{0,5}>+", "$&>", RegexOptions.Multiline);
				msg = Regex.Replace(msg, @"(?m)^(?!\s*[-\w\.]{0,5}>|\s*$)", shortname + ">");
				msg = "Здравствуйте, " + nick + ", Вы писали:\n\n" + msg;

				return msg;
			}
		}
	}
}
