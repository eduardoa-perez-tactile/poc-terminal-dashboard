namespace Tva.Core;

public readonly record struct ScreenId(string Value)
{
    public override string ToString() => Value;

    public static implicit operator ScreenId(string value) => new(value);
}
