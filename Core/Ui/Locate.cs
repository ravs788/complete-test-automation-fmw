namespace Core.Ui;

/// <summary>How an element is found, independent of the driver (R42). Create with the static methods.</summary>
public sealed record Locate
{
    private Locate(LocateKind kind, string value, string? name = null)
    {
        Kind = kind;
        Value = value;
        Name = name;
    }

    public LocateKind Kind { get; }

    /// <summary>CSS selector, role name or placeholder text, depending on <see cref="Kind"/>.</summary>
    public string Value { get; }

    /// <summary>Accessible name for <see cref="LocateKind.Role"/> (substring, case-insensitive), or null for any.</summary>
    public string? Name { get; }

    /// <summary>Element whose <paramref name="attribute"/> equals <paramref name="id"/>, e.g. <c>data-test="login-button"</c>.</summary>
    public static Locate TestId(string attribute, string id) => new(LocateKind.Css, $"[{attribute}=\"{id}\"]");

    public static Locate Css(string selector) => new(LocateKind.Css, selector);

    /// <summary>Element with an ARIA role and, optionally, an accessible name (e.g. a button labelled "Remove").</summary>
    public static Locate Role(UiRole role, string? name = null) => new(LocateKind.Role, role.ToString(), name);

    public static Locate Placeholder(string text) => new(LocateKind.Placeholder, text);

    public override string ToString() => Kind switch
    {
        LocateKind.Role => Name is null ? $"role={Value}" : $"role={Value}[name~\"{Name}\"]",
        LocateKind.Placeholder => $"placeholder=\"{Value}\"",
        _ => Value,
    };
}

public enum LocateKind
{
    Css,
    Role,
    Placeholder,
}

/// <summary>The ARIA roles the page objects use; add more as needed.</summary>
public enum UiRole
{
    Button,
    Checkbox,
    Link,
    Textbox,
    Heading,
}
