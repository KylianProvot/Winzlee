// Human readable gesture summaries using the UI language, without changing persisted enum values.

using Filee.Core.Localization;
using Filee.Core.Settings;

namespace Filee.App.Services;

public static class GestureText
{
    public static string Modifiers(ModifierKeys modifiers, ILocalizer? loc = null)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Ctrl)) parts.Add(OperatingSystem.IsMacOS() ? "⌃" : "Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add(OperatingSystem.IsMacOS() ? "⌥ Option" : "Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add(loc?["triggers.modifier.shift"] ?? "Shift");
        if (modifiers.HasFlag(ModifierKeys.Meta)) parts.Add(OperatingSystem.IsMacOS() ? "⌘" : "Win");
        return parts.Count == 0 ? "—" : string.Join(" + ", parts);
    }

    public static string Describe(TriggerGesture gesture, ILocalizer? loc = null)
    {
        var mods = Modifiers(gesture.Modifiers, loc);
        if (gesture.Kind == TriggerKind.KeyChord)
            return gesture.Modifiers == ModifierKeys.None ? gesture.Key ?? "—" : $"{mods} + {gesture.Key ?? "?"}";
        var button = gesture.Kind == TriggerKind.Click && gesture.Button == TriggerMouseButton.Right && loc is not null
            ? loc["triggers.click_right"]
            : loc is null ? gesture.Button.ToString() : loc[$"triggers.button.{gesture.Button}"];
        return $"{mods} + {button}";
    }
}
