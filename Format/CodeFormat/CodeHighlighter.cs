using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Rsdn.Framework.Formatting.CodeFormat;

namespace Rsdn.Framework.Formatting
{
    /// <summary>
    /// Подсветка синтаксиса кода с использованием Trie для ключевых слов
    /// и регулярных выражений для остальных паттернов.
    /// </summary>
    public class CodeHighlighter
    {
        private readonly SyntaxDefinition _syntax;
        private readonly List<TrieKeywordMatcher> _keywordMatchers = new();
        private readonly List<RegexPattern> _regexPatterns = new();
        private readonly Regex? _combinedRegex;
        private readonly string[] _groupNames;

        /// <summary>
        /// Создать подсветчик из определения синтаксиса.
        /// </summary>
        public CodeHighlighter(SyntaxDefinition syntax)
        {
            _syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));

            var regexBuilder = new StringBuilder();
            var groupIndex = 0;
            var groupList = new List<string>();

            foreach (var pattern in syntax.Patterns)
            {
                if (pattern.Type == PatternType.Keyword && pattern.Keywords.Count > 0)
                {
                    // Для ключевых слов используем Trie
                    _keywordMatchers.Add(new TrieKeywordMatcher(
                        pattern.Keywords,
                        pattern.Name,
                        pattern.Prefix,
                        pattern.Postfix,
                        pattern.IgnoreCase));
                }
                else if (pattern.Expressions.Count > 0)
                {
                    // Для regex паттернов строим комбинированное выражение
                    foreach (var expr in pattern.Expressions)
                    {
                        if (regexBuilder.Length > 0)
                            regexBuilder.Append('|');

                        regexBuilder.Append($"(?<{SanitizeGroupName(pattern.Name)}_{groupIndex}>{expr})");
                        groupList.Add(pattern.Name);
                        groupIndex++;
                    }
                }
            }

            _groupNames = groupList.ToArray();

            if (regexBuilder.Length > 0)
            {
                var options = ParseRegexOptions(syntax.Options);
                _combinedRegex = new Regex(regexBuilder.ToString(), options);
            }
        }

        /// <summary>
        /// Загрузить подсветчик из JSON-файла.
        /// </summary>
        public static CodeHighlighter FromJsonFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var syntax = JsonSerializer.Deserialize<SyntaxDefinition>(json);
            return new CodeHighlighter(syntax ?? throw new InvalidOperationException("Invalid syntax definition"));
        }

        /// <summary>
        /// Загрузить подсветчик из JSON-потока.
        /// </summary>
        public static CodeHighlighter FromJsonStream(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var syntax = JsonSerializer.Deserialize<SyntaxDefinition>(json);
            return new CodeHighlighter(syntax ?? throw new InvalidOperationException("Invalid syntax definition"));
        }

        /// <summary>
        /// Подсветить код.
        /// </summary>
        public string Highlight(string code)
        {
            if (string.IsNullOrEmpty(code))
                return code;

            var result = code;

            // Сначала применяем regex паттерны (комментарии, строки)
            if (_combinedRegex != null)
            {
                result = _combinedRegex.Replace(result, match =>
                {
                    // Находим имя группы, которая совпала
                    for (int i = 0; i < _groupNames.Length; i++)
                    {
                        var groupName = $"{SanitizeGroupName(_groupNames[i])}_{i}";
                        var group = match.Groups[groupName];
                        if (group.Success)
                        {
                            return $"<{_groupNames[i]}>{match.Value}</{_groupNames[i]}>";
                        }
                    }
                    return match.Value;
                });
            }

            // Затем применяем Trie для ключевых слов
            // Но только к частям, которые ещё не выделены
            foreach (var matcher in _keywordMatchers)
            {
                result = HighlightKeywords(result, matcher);
            }

            return result;
        }

        private string HighlightKeywords(string input, TrieKeywordMatcher matcher)
        {
            // Разбиваем на части: уже выделенные и не выделенные
            // Применяем matcher только к не выделенным частям
            var result = new StringBuilder(input.Length * 2);
            var pos = 0;

            while (pos < input.Length)
            {
                // Ищем следующий тег
                var tagStart = input.IndexOf('<', pos);

                if (tagStart == -1)
                {
                    // Больше тегов нет, подсвечиваем остаток
                    result.Append(matcher.Highlight(input.Substring(pos)));
                    break;
                }

                // Подсвечиваем часть до тега
                if (tagStart > pos)
                {
                    result.Append(matcher.Highlight(input.Substring(pos, tagStart - pos)));
                }

                // Находим конец тега
                var tagEnd = input.IndexOf('>', tagStart);
                if (tagEnd == -1)
                {
                    result.Append(input.Substring(pos));
                    break;
                }

                // Проверяем, это открывающий или закрывающий тег
                var tagContent = input.Substring(tagStart, tagEnd - tagStart + 1);

                if (tagContent.StartsWith("</"))
                {
                    // Закрывающий тег
                    var endTagEnd = input.IndexOf('>', tagStart);
                    result.Append(input, tagStart, endTagEnd - tagStart + 1);
                    pos = endTagEnd + 1;
                }
                else
                {
                    // Открывающий тег - находим соответствующий закрывающий
                    var tagName = ExtractTagName(tagContent);
                    var closeTag = $"</{tagName}>";

                    // Ищем закрывающий тег
                    var closePos = input.IndexOf(closeTag, tagEnd + 1, StringComparison.Ordinal);
                    if (closePos == -1)
                    {
                        result.Append(input.Substring(tagStart));
                        break;
                    }

                    // Копируем весь блок как есть
                    var blockEnd = closePos + closeTag.Length;
                    result.Append(input, tagStart, blockEnd - tagStart);
                    pos = blockEnd;
                }
            }

            return result.ToString();
        }

        private static string ExtractTagName(string tagContent)
        {
            // <span class='kw'> -> span
            // <kw> -> kw
            var start = tagContent.IndexOf('<') + 1;
            var end = tagContent.IndexOfAny(new[] { ' ', '>', '\t' }, start);
            if (end == -1)
                end = tagContent.Length - 1;

            return tagContent.Substring(start, end - start);
        }

        private static string SanitizeGroupName(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_')
                    sb.Append(ch);
                else
                    sb.Append('_');
            }
            return sb.ToString();
        }

        private static RegexOptions ParseRegexOptions(string? options)
        {
            var result = RegexOptions.None;

            if (string.IsNullOrEmpty(options))
                return result;

            // Парсим опции в формате (?in) или (?inm)
            // i - IgnoreCase
            // n - ExplicitCapture
            // m - Multiline
            // s - Singleline

            if (options.IndexOf('i') >= 0)
                result |= RegexOptions.IgnoreCase;
            if (options.IndexOf('n') >= 0)
                result |= RegexOptions.ExplicitCapture;
            if (options.IndexOf('m') >= 0)
                result |= RegexOptions.Multiline;
            if (options.IndexOf('s') >= 0)
                result |= RegexOptions.Singleline;

            return result;
        }

        private class RegexPattern
        {
            public string Name { get; set; } = "";
            public Regex Regex { get; set; } = null!;
        }
    }
}