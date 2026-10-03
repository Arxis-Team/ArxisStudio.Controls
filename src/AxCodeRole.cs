namespace ArxisStudio.Controls;

/// <summary>
/// Что разбор хозяина сказал о куске кода: по роли <see cref="AxCodeView"/> берёт кисть темы.
/// </summary>
/// <remarks>
/// Роль называет смысл, а не цвет: цвет меняется с темой, смысл остаётся. Ролей столько, сколько
/// различает разметка интерфейса — XAML и её родня; язык с ключевыми словами получит свои роли
/// добавкой, а не перекраской этих.
/// </remarks>
public enum AxCodeRole
{
    /// <summary>Текст без подсветки: содержимое элемента, пробелы, знаки.</summary>
    Text,

    /// <summary>Имя элемента вместе с угловыми скобками: <c>&lt;Button</c>, <c>/&gt;</c>.</summary>
    Tag,

    /// <summary>Имя свойства: <c>Content</c>, <c>Grid.Row</c>.</summary>
    Attribute,

    /// <summary>Значение в кавычках вместе с кавычками.</summary>
    String,

    /// <summary>Комментарий вместе с ограничителями.</summary>
    Comment,

    /// <summary>Расширение разметки: <c>{Binding</c> и закрывающая скобка.</summary>
    Extension,

    /// <summary>Приставка пространства имён: <c>local:</c> в <c>local:Badge</c>.</summary>
    Prefix,

    /// <summary>Директива языка разметки: <c>x:Name</c>, <c>x:Class</c>, <c>xmlns</c>.</summary>
    Directive,

    /// <summary>Текст, который разбор не понял: цвет ошибки и подчёркивание.</summary>
    Error,
}
