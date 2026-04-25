# RSDN Formatter

Библиотека форматирования сообщений RSDN (Russian Software Developers Network) с подсветкой синтаксиса кода.

## Возможности

- **BBCode-подобный синтаксис** — привычные теги `[b]`, `[i]`, `[url]`, `[img]` и др.
- **Подсветка кода** — поддержка 20+ языков программирования
- **Смайлики** — классические смайлики RSDN
- **Таблицы** — полноценная поддержка таблиц
- **Цитирование** — блочные и inline-цитаты
- **Автоматические ссылки** — распознавание URL и email

## Документация

📖 [Справочник по языку разметки RSDN](docs/markup-reference.md)

## Быстрый старт

```csharp
using Rsdn.Framework.Formatting;

var formatter = new TextFormatter();
var html = formatter.Format("[b]Жирный текст[/b]");
Console.WriteLine(html);
// <b>Жирный текст</b>
```

## Поддерживаемые теги

| Категория      | Теги                                         |
|----------------|----------------------------------------------|
| Форматирование | `[b]`, `[i]`, `[u]`, `[s]`, `[sub]`, `[sup]` |
| Ссылки         | `[url]`, `[email]`                           |
| Изображения    | `[img]`, `[img=small]`, `[img=large]`        |
| Цитирование    | `[quote]`, `[q]`, `A>`                       |
| Структура      | `[h1]`-`[h6]`, `[hr]`, `[cut]`               |
| Списки         | `[list]`, `[list=1]`, `[list=a]`             |
| Таблицы        | `[t]`, `[tr]`, `[td]`, `[th]`                |
| Код            | `[code]`, `[code=c#]`, `[code=python]`       |

## Подсветка кода

Поддерживаемые языки: C#, C/C++, Visual Basic, Java, JavaScript, TypeScript, Python, SQL, XML/HTML, CSS, PHP, Ruby, Go, Rust, Assembler, Pascal/Delphi, Nemerle, Nitra, Objective-C.

## Лицензия

[MIT](LICENSE.md)