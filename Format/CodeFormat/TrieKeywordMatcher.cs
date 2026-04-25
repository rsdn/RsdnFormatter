using System.Collections.Generic;
using System.Text;

namespace Rsdn.Framework.Formatting.CodeFormat;

/// <summary>
/// Быстрый поиск множества ключевых слов с использованием Trie-структуры.
/// Оптимизирован для подсветки синтаксиса.
/// </summary>
public class TrieKeywordMatcher
{
    private readonly TrieNode _root = new();
    private readonly bool _ignoreCase;
    private readonly KeywordBoundary _prefix;
    private readonly KeywordBoundary _postfix;
    private readonly string _groupName;

    /// <summary>
    /// Создать matcher для списка ключевых слов.
    /// </summary>
    /// <param name="keywords">Список ключевых слов</param>
    /// <param name="groupName">Имя группы (CSS-класс)</param>
    /// <param name="prefix">Граница перед ключевым словом</param>
    /// <param name="postfix">Граница после ключевого слова</param>
    /// <param name="ignoreCase">Игнорировать регистр</param>
    public TrieKeywordMatcher(
        IEnumerable<string> keywords,
        string groupName,
        KeywordBoundary prefix = KeywordBoundary.None,
        KeywordBoundary postfix = KeywordBoundary.None,
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
            // Проверяем префикс (границу перед словом)
            if (!CheckPrefix(input, i))
            {
                result.Append(input[i]);
                i++;
                continue;
            }

            // Пытаемся найти ключевое слово
            var (found, word, endPos) = FindKeyword(input, i);

            if (found && word != null)
            {
                // Проверяем постфикс (границу после слова)
                var (postfixOk, highlightEnd) = CheckPostfix(input, endPos);
                if (postfixOk)
                {
                    result.Append('<').Append(_groupName).Append('>');
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
                lastMatch = (true, node.OriginalWord, i);
        }

        return lastMatch;
    }

    /// <summary>
    /// Проверка границы перед ключевым словом.
    /// </summary>
    private bool CheckPrefix(string input, int pos)
    {
        return _prefix switch
        {
            KeywordBoundary.None => true,

            KeywordBoundary.WordBoundary => CheckWordBoundaryBefore(input, pos),

            KeywordBoundary.Dot => CheckDotBefore(input, pos),

            KeywordBoundary.DotOrWord => CheckDotBefore(input, pos) || CheckWordBoundaryBefore(input, pos),

            KeywordBoundary.HashWithSpace => CheckHashWithSpaceBefore(input, pos),

            KeywordBoundary.AtSignOrWord => CheckAtSignBefore(input, pos) || CheckWordBoundaryBefore(input, pos),

            KeywordBoundary.DotOrAtOrWord => CheckDotBefore(input, pos) ||
                                            CheckAtSignBefore(input, pos) ||
                                            CheckWordBoundaryBefore(input, pos),

            KeywordBoundary.DoubleQuestion => CheckDoubleQuestionBefore(input, pos),

            KeywordBoundary.NotAmpersand => CheckNotAmpersandBefore(input, pos),

            KeywordBoundary.WordBoundaryOrDoubleQuestion => CheckWordBoundaryBefore(input, pos) ||
                                                            CheckDoubleQuestionBefore(input, pos),

            KeywordBoundary.ExclamationAndWordBoundary => CheckWordBoundaryBefore(input, pos),

            KeywordBoundary.OpenParen => CheckOpenParenBefore(input, pos),

            _ => true
        };
    }

    /// <summary>
    /// Проверка границы после ключевого слова.
    /// Возвращает (успех, позиция конца подсветки).
    /// </summary>
    private (bool success, int highlightEnd) CheckPostfix(string input, int pos)
    {
        return _postfix switch
        {
            KeywordBoundary.None => (true, pos),

            KeywordBoundary.WordBoundary => (CheckWordBoundaryAfter(input, pos), pos),

            KeywordBoundary.Dot => (CheckDotAfter(input, pos), pos),

            KeywordBoundary.DotOrWord => (CheckDotAfter(input, pos) || CheckWordBoundaryAfter(input, pos), pos),

            KeywordBoundary.HashWithSpace => (CheckWordBoundaryAfter(input, pos), pos), // Для postfix не имеет смысла

            KeywordBoundary.AtSignOrWord => (CheckAtSignAfter(input, pos) || CheckWordBoundaryAfter(input, pos), pos),

            KeywordBoundary.DotOrAtOrWord => (CheckDotAfter(input, pos) ||
                                              CheckAtSignAfter(input, pos) ||
                                              CheckWordBoundaryAfter(input, pos), pos),

            KeywordBoundary.DoubleQuestion => (CheckWordBoundaryAfter(input, pos), pos), // Для postfix не имеет смысла

            KeywordBoundary.NotAmpersand => (CheckWordBoundaryAfter(input, pos), pos), // Для postfix не имеет смысла

            KeywordBoundary.WordBoundaryOrDoubleQuestion => (CheckWordBoundaryAfter(input, pos), pos),

            KeywordBoundary.ExclamationAndWordBoundary => CheckExclamationAndWordBoundary(input, pos),

            _ => (true, pos)
        };
    }

    /// <summary>
    /// Проверка !\b - восклицательный знак как часть ключевого слова, затем граница слова.
    /// Используется для макросов Rust: assert!, fail!
    /// </summary>
    private static (bool success, int highlightEnd) CheckExclamationAndWordBoundary(string input, int pos)
    {
        // Должен быть ! после ключевого слова
        if (pos >= input.Length || input[pos] != '!')
            return (false, pos);

        // После ! должна быть граница слова
        if (pos + 1 >= input.Length)
            return (true, pos + 1); // ! в конце строки

        var afterBang = input[pos + 1];
        return !IsWordChar(afterBang) ? (true, pos + 1) : (false, pos);
    }

    #region Prefix checks

    /// <summary>
    /// Проверка границы слова перед позицией (\b).
    /// </summary>
    private static bool CheckWordBoundaryBefore(string input, int pos)
    {
        if (pos == 0)
            return true;

        var prevChar = input[pos - 1];
        return !IsWordChar(prevChar);
    }

    /// <summary>
    /// Проверка границы слова после позиции (\b).
    /// </summary>
    private static bool CheckWordBoundaryAfter(string input, int pos)
    {
        if (pos >= input.Length)
            return true;

        var nextChar = input[pos];
        return !IsWordChar(nextChar);
    }

    /// <summary>
    /// Проверка точки перед позицией (\.).
    /// </summary>
    private static bool CheckDotBefore(string input, int pos)
    {
        if (pos == 0)
            return false;

        return input[pos - 1] == '.';
    }

    /// <summary>
    /// Проверка точки после позиции.
    /// </summary>
    private static bool CheckDotAfter(string input, int pos)
    {
        if (pos >= input.Length)
            return false;

        return input[pos] == '.';
    }

    /// <summary>
    /// Проверка решётки с пробелами перед позицией (#\s*).
    /// Ключевое слово должно следовать за # и возможными пробелами.
    /// </summary>
    private static bool CheckHashWithSpaceBefore(string input, int pos)
    {
        if (pos == 0)
            return false;

        // Ищем # перед позицией, пропуская пробелы
        var i = pos - 1;
        while (i >= 0 && char.IsWhiteSpace(input[i]))
            i--;

        return i >= 0 && input[i] == '#';
    }

    /// <summary>
    /// Проверка @ перед позицией.
    /// </summary>
    private static bool CheckAtSignBefore(string input, int pos)
    {
        if (pos == 0)
            return false;

        return input[pos - 1] == '@';
    }

    /// <summary>
    /// Проверка @ после позиции.
    /// </summary>
    private static bool CheckAtSignAfter(string input, int pos)
    {
        if (pos >= input.Length)
            return false;

        return input[pos] == '@';
    }

    /// <summary>
    /// Проверка ?? перед позицией.
    /// </summary>
    private static bool CheckDoubleQuestionBefore(string input, int pos)
    {
        if (pos < 2)
            return false;

        return input[pos - 2] == '?' && input[pos - 1] == '?';
    }

    /// <summary>
    /// Проверка что перед позицией не &.
    /// Используется для lt/gt чтобы не матчить HTML-сущности.
    /// </summary>
    private static bool CheckNotAmpersandBefore(string input, int pos)
    {
        if (pos == 0)
            return true;

        return input[pos - 1] != '&';
    }

    /// <summary>
    /// Проверка открывающей скобки перед позицией \(
    /// Используется для Lisp: (defun, (let, (if и т.д.
    /// </summary>
    private static bool CheckOpenParenBefore(string input, int pos)
    {
        if (pos == 0)
            return false;

        return input[pos - 1] == '(';
    }

    #endregion

    /// <summary>
    /// Проверка, является ли символ частью слова (буква, цифра или подчёркивание).
    /// </summary>
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