using System;
using System.Collections.Generic;
using System.Text;

namespace Rsdn.Framework.Formatting
{
    /// <summary>
    /// Быстрый поиск множества ключевых слов с использованием Trie-структуры.
    /// Оптимизирован для подсветки синтаксиса.
    /// </summary>
    public class TrieKeywordMatcher
    {
        private readonly TrieNode _root = new();
        private readonly bool _ignoreCase;
        private readonly string? _prefix;
        private readonly string? _postfix;
        private readonly string _groupName;

        /// <summary>
        /// Создать matcher для списка ключевых слов.
        /// </summary>
        /// <param name="keywords">Список ключевых слов</param>
        /// <param name="groupName">Имя группы (CSS-класс)</param>
        /// <param name="prefix">Префикс (например, \b для границы слова)</param>
        /// <param name="postfix">Постфикс</param>
        /// <param name="ignoreCase">Игнорировать регистр</param>
        public TrieKeywordMatcher(
            IEnumerable<string> keywords,
            string groupName,
            string? prefix = null,
            string? postfix = null,
            bool ignoreCase = false)
        {
            _groupName = groupName;
            _prefix = prefix;
            _postfix = postfix;
            _ignoreCase = ignoreCase;

            foreach (var keyword in keywords)
            {
                if (string.IsNullOrEmpty(keyword))
                    continue;

                var word = ignoreCase ? keyword.ToLowerInvariant() : keyword;
                AddToTrie(word, keyword);
            }
        }

        private void AddToTrie(string normalizedWord, string originalWord)
        {
            var node = _root;
            foreach (var ch in normalizedWord)
            {
                if (!node.Children.TryGetValue(ch, out var child))
                {
                    child = new TrieNode();
                    node.Children[ch] = child;
                }
                node = child;
            }
            node.IsTerminal = true;
            node.OriginalWord = originalWord;
        }

        /// <summary>
        /// Найти все ключевые слова в тексте и выделить их тегами.
        /// </summary>
        /// <param name="input">Входной текст</param>
        /// <returns>Текст с выделенными ключевыми словами</returns>
        public string Highlight(string input)
        {
            if (string.IsNullOrEmpty(input) || _root.Children.Count == 0)
                return input;

            var result = new StringBuilder(input.Length * 2);
            var i = 0;

            while (i < input.Length)
            {
                // Проверяем префикс (например, границу слова)
                if (_prefix != null && !CheckPrefix(input, i))
                {
                    result.Append(input[i]);
                    i++;
                    continue;
                }

                // Пытаемся найти ключевое слово
                var (found, word, endPos) = FindKeyword(input, i);

                if (found && word != null)
                {
                    // Проверяем постфикс (например, границу слова после)
                    if (_postfix == null || CheckPostfix(input, endPos))
                    {
                        result.Append('<').Append(_groupName).Append('>');
                        // Для постфикса !\b включаем ! в подсвечиваемый текст
                        var highlightEnd = (_postfix == @"!\b" && endPos < input.Length && input[endPos] == '!')
                            ? endPos + 1
                            : endPos;
                        result.Append(input.Substring(i, highlightEnd - i));
                        result.Append("</").Append(_groupName).Append('>');
                        i = highlightEnd;
                        continue;
                    }
                }

                result.Append(input[i]);
                i++;
            }

            return result.ToString();
        }

        private (bool found, string? word, int endPos) FindKeyword(string input, int startPos)
        {
            var node = _root;
            var lastMatch = (found: false, word: (string?)null, endPos: startPos);
            var i = startPos;

            while (i < input.Length)
            {
                var ch = _ignoreCase ? char.ToLowerInvariant(input[i]) : input[i];

                if (!node.Children.TryGetValue(ch, out var nextNode))
                    break;

                node = nextNode;
                i++;

                if (node.IsTerminal)
                {
                    lastMatch = (true, node.OriginalWord, i);
                }
            }

            return lastMatch;
        }

        private bool CheckPrefix(string input, int pos)
        {
            // \b - граница слова
            if (_prefix == @"\b")
            {
                if (pos == 0)
                    return true;

                var prevChar = input[pos - 1];
                return !IsWordChar(prevChar);
            }

            // Для других префиксов можно добавить логику
            return true;
        }

        private bool CheckPostfix(string input, int pos)
        {
            // \b - граница слова
            if (_postfix == @"\b")
            {
                if (pos >= input.Length)
                    return true;

                var nextChar = input[pos];
                return !IsWordChar(nextChar);
            }

            // !\b - восклицательный знак как часть ключевого слова, затем граница слова
            if (_postfix == @"!\b")
            {
                if (pos >= input.Length)
                    return false;

                if (input[pos] != '!')
                    return false;

                // После ! должна быть граница слова
                if (pos + 1 >= input.Length)
                    return true;

                var afterBang = input[pos + 1];
                return !IsWordChar(afterBang);
            }

            return true;
        }

        private static bool IsWordChar(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_';
        }

        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; } = new();
            public bool IsTerminal { get; set; }
            public string? OriginalWord { get; set; }
        }
    }
}