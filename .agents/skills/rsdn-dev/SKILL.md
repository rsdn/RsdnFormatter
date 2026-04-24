---
name: rsdn-dev
description: Правила разработки RsdnFormatter
---

# Правила разработки

## Организация файлов

- **Один тип верхнего уровня — один файл** — каждый публичный класс, структура, интерфейс, enum в отдельном файле
- **File-scoped namespaces** для всех новых файлов

```csharp
// ✅ Правильно
namespace Rsdn.Framework.Formatting.BBCode.Nodes;

public class TextNode : Node { }

// ❌ Неправильно (для новых файлов)
namespace Rsdn.Framework.Formatting.BBCode.Nodes
{
    public class TextNode : Node { }
}
```

## C# 14 Features

### Primary Constructors

Использовать для классов с обязательной инициализацией:

```csharp
public class TagNode(string tagName, string? attribute = null) : Node
{
    public string TagName { get; } = tagName.ToLowerInvariant();
    public string? Attribute { get; } = attribute;
    public List<Node> Children { get; } = [];
}
```

### Collection Expressions

```csharp
// ✅ Правильно
public List<Node> Children { get; } = [];

// ❌ Неправильно
public List<Node> Children { get; } = new List<Node>();
```

### Target-typed New

```csharp
// ✅ Правильно
var document = new DocumentNode();

// Когда тип очевиден из контекста
Dictionary<string, string> map = new() { ["key"] = "value" };
```

### Pattern Matching

```csharp
// Switch expression
public override string ToString() => Type switch
{
    TokenType.Text => $"Text: [{TextRange.Start}..{TextRange.End}]",
    TokenType.OpenTag => $"OpenTag: [{TagNameRange.Start}..{TagNameRange.End}]",
    _ => $"Unknown: {Type}"
};

// Pattern matching с is
if (child is TextNode textNode)
{
    var text = textNode.Text;
}
```

## Типы данных

### ref struct

Использовать для работы с `ReadOnlySpan<T>` без аллокаций:

```csharp
public ref struct Tokenizer(ReadOnlySpan<char> text)
{
    private readonly ReadOnlySpan<char> _text = text;
    public int Position { get; private set; } = 0;
}
```

### readonly struct

Использовать для immutable структур данных:

```csharp
public readonly struct Token
{
    public TokenType Type { get; }
    public int Position { get; }
}
```

### Nullable Reference Types

Проект использует NRT — явно указывать `?` для nullable типов:

```csharp
public string? Attribute { get; }  // Может быть null
public string TagName { get; }      // Не может быть null
```

## Паттерны проектирования

### Visitor Pattern

Использовать для обработки AST:

```csharp
public interface INodeVisitor<TContext>
{
    void Visit(DocumentNode node, TContext ctx);
    void Visit(TagNode node, TContext ctx);
    void Visit(TextNode node, TContext ctx);
}

public abstract class Node
{
    public abstract void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx);
}
```

### Token-based Parsing

Разделять лексический и синтаксический анализ:

```
Текст → Tokenizer → Токены → Parser → AST → Renderer → HTML
```

### TextRange вместо строк

Использовать диапазоны в токенах для избежания аллокаций:

```csharp
public readonly struct TextRange
{
    public int Start { get; }
    public int End { get; }
    public int Length => End - Start;
}

// Преобразование в строку только при необходимости
public string? GetTextString(ReadOnlySpan<char> source)
{
    var span = source.Slice(TextRange.Start, TextRange.Length);
    return span.IsEmpty ? null : span.ToString();
}
```

## Тестирование

- **Фреймворк**: NUnit
- **Именование**: `MethodName_Scenario_ExpectedResult`
- **Организация**: по `#region` категориям (Tokenizer Tests, Parser Tests, Integration Tests)

```csharp
[Test]
public void Parser_BoldTag_ReturnsTagNode()
{
    var parser = new Parser("[b]text[/b]");
    var doc = parser.Parse();
    
    Assert.AreEqual(1, doc.Children.Count);
    Assert.IsInstanceOf<TagNode>(doc.Children[0]);
}