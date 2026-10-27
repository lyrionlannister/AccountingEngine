using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AccountingEngine.Desktop.Shared.Controls;

public sealed class Badge : TemplatedControl
{
    public static readonly StyledProperty<string> ToneProperty =
        AvaloniaProperty.Register<Badge, string>(nameof(Tone), "neutral");

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Badge, string>(nameof(Text), string.Empty);

    public string Tone
    {
        get => GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
