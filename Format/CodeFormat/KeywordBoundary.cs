namespace Rsdn.Framework.Formatting.CodeFormat;

/// <summary>
/// Тип границы для матчинга ключевых слов.
/// Заменяет regex-выражения prefix/postfix из старых XML файлов.
/// </summary>
public enum KeywordBoundary
{
    /// <summary>
    /// Нет проверки границы.
    /// </summary>
    None,

    /// <summary>
    /// Граница слова (\b).
    /// Символ до/после не является буквой, цифрой или подчёркиванием.
    /// </summary>
    WordBoundary,

    /// <summary>
    /// Точка перед ключевым словом (\.).
    /// Используется для директив ассемблера (.alpha, .break и т.д.)
    /// </summary>
    Dot,

    /// <summary>
    /// Точка ИЛИ граница слова (\.|\b).
    /// Ключевое слово может начинаться с точки или быть на границе слова.
    /// </summary>
    DotOrWord,

    /// <summary>
    /// Решётка с возможными пробелами (#\s*).
    /// Используется для директив препроцессора (#define, #include и т.д.)
    /// </summary>
    HashWithSpace,

    /// <summary>
    /// Собака ИЛИ граница слова (@|\b).
    /// Используется для переменных вроде @curseg, @filename.
    /// </summary>
    AtSignOrWord,

    /// <summary>
    /// Точка, собака ИЛИ граница слова (\.|@|\b).
    /// Используется для конструкций вроде .code, @code, code.
    /// </summary>
    DotOrAtOrWord,

    /// <summary>
    /// Двойной вопросительный знак (\?\?).
    /// Используется для ??date, ??filename, ??time.
    /// </summary>
    DoubleQuestion,

    /// <summary>
    /// Не после амперсанда (?<!&).
    /// Используется для lt/gt чтобы не матчить < >.
    /// </summary>
    NotAmpersand,

    /// <summary>
    /// Граница слова ИЛИ двойной вопрос (\b|\?\?).
    /// Используется для version, которая может быть ??version или просто version.
    /// </summary>
    WordBoundaryOrDoubleQuestion,

    /// <summary>
    /// Восклицательный знак с границей слова после (!\b).
    /// Используется для макросов вроде assert!, fail! в Rust.
    /// Восклицательный знак включается в подсвечиваемый текст.
    /// </summary>
    ExclamationAndWordBoundary,

    /// <summary>
    /// Открывающая скобка перед ключевым словом (\().
    /// Используется для Lisp: (defun, (let, (if и т.д.
    /// </summary>
    OpenParen
}
