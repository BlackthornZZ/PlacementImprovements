using BuildImprovements.Patches;
using MelonLoader;
using MelonLoader.Preferences;
using Starlight.Utils;
using System.Configuration;
using UnityEngine;

namespace BuildImprovements.Preferences;
public static class PreferenceDirector
{
    public static readonly MelonPreferences_Category ColorPreferences = MelonPreferences.CreateCategory("Placement Improvements - Colors"); 
    public static readonly MelonPreferences_Category SettingPreferences = MelonPreferences.CreateCategory("Placement Improvements - Settings");
    public static readonly MelonPreferences_Category KeybindPreferences = MelonPreferences.CreateCategory("Placement Improvements - Keybinds");

    // Enabled/disabled flags for individual build improvements.
    internal static bool bAllowSlopedPlacementAngle
    {
        get => SettingPreferences.GetEntry<bool>("SlopedPlacementAngles").Value;
        private set => SetConfigValue<bool>(SettingPreferences, "SlopedPlacementAngles", value);
    }
    internal static bool bIgnorePlayerGroundedState
    {
        get => SettingPreferences.GetEntry<bool>("IgnorePlayerGrounded").Value;
        private set => SetConfigValue<bool>(SettingPreferences, "IgnorePlayerGrounded", value);
    }
    internal static bool bAllowClipping
    {
        get => SettingPreferences.GetEntry<bool>("AllowClipping").Value;
        private set => SetConfigValue<bool>(SettingPreferences, "AllowClipping", value);
    }
    internal static bool bAllowAdvancedMovement
    {
        get => SettingPreferences.GetEntry<bool>("AllowAdvancedMovement").Value;
        private set => SetConfigValue<bool>(SettingPreferences, "AllowAdvancedMovement", value);
    }

    // Placement colors
    // Public for use by SR2MP.
    // These are exclusively RGB colors; you will need to have the alpha channel depend on the material you're changing.
    public static Color ValidColor
    {
        get => ParseColor(ColorPreferences.GetEntry<string>("ValidColor").Value,
            Fallback: ParseColor(ColorPreferences.GetEntry<string>("ValidColor").DefaultValue));
        set => SetConfigValue<string>(ColorPreferences, "ValidColor", string.Format("{0},{1},{2}", value.r, value.g, value.b));
    }
    public static Color AlmostValidColor
    {
        get => ParseColor(ColorPreferences.GetEntry<string>("AlmostValidColor").Value,
            Fallback: ParseColor(ColorPreferences.GetEntry<string>("AlmostValidColor").DefaultValue));
        set => SetConfigValue<string>(ColorPreferences, "AlmostValidColor", string.Format("{0},{1},{2}", value.r, value.g, value.b));
    }
    public static Color InvalidColor
    {
        get => ParseColor(ColorPreferences.GetEntry<string>("InvalidColor").Value,
            Fallback: ParseColor(ColorPreferences.GetEntry<string>("InvalidColor").DefaultValue));
        set => SetConfigValue<string>(ColorPreferences, "InvalidColor", string.Format("{0},{1},{2}", value.r, value.g, value.b));
    }

    // Keybinds
    internal static KeyCode PlacementLockBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("LockPlacementBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "LockPlacementBind", value);
    }
    internal static KeyCode NudgeUpBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("NudgeUpBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "NudgeUpBind", value);
    }
    internal static KeyCode NudgeDownBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("NudgeDownBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "NudgeDownBind", value);
    }
    internal static KeyCode NudgeForwardBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("NudgeForwardBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "NudgeForwardBind", value);
    }
    internal static KeyCode NudgeLeftBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("NudgeLeftBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "NudgeLeftBind", value);
    }
    internal static KeyCode NudgeRightBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("NudgeRightBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "NudgeRightBind", value);
    }
    internal static KeyCode NudgeBackwardBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("NudgeBackwardBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "NudgeBackwardBind", value);
    }
    internal static KeyCode SmoothNudgeBind 
    {
        get => KeybindPreferences.GetEntry<KeyCode>("SmoothNudgeBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "SmoothNudgeBind", value);
    }
    internal static KeyCode ResetPlacementBind
    {
        get => KeybindPreferences.GetEntry<KeyCode>("ResetBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "ResetBind", value);
    }

    // Nudging features
    internal static bool bSmoothNudge => InputEUtil.OnKey(SmoothNudgeBind);
    internal static float NudgeIncrementScale
    {
        get => SettingPreferences.GetEntry<float>("NudgeIncrement").Value;
        private set => SetConfigValue<float>(SettingPreferences, "NudgeIncrement", value);
    }
    internal static float NudgeSpeed
    {
        get => SettingPreferences.GetEntry<float>("NudgeSpeed").Value;
        private set => SetConfigValue<float>(SettingPreferences, "NudgeSpeed", value);
    }



    // Called upon in Main::OnLateInitialize to set up the preference categories.
    internal static void CreatePreferences()
    {
        // Client-side colors! Don't sync these ones, each client will have their own and they do not need to show up to other clients.
        // == Colors ==
        ColorPreferences.CreateEntry(
            identifier: "ValidColor",
            default_value: "0,153,171",
            display_name: "Valid Placement Color",
            description: "The color the placement preview should take on when it is valid.\nFormat is \"R,G,B\". The alpha channel is determined by the source material.",
            is_hidden: false
            );

        ColorPreferences.CreateEntry(
            identifier: "AlmostValidColor",
            default_value: "234,120,24",
            display_name: "Clipping Placement Color",
            description: "The color the placement preview should take on when the mod overrides placeability while it is clipping, floating, etc.\nFormat is \"R,G,B\". The alpha channel is determined by the source material.",
            is_hidden: false
            );

        ColorPreferences.CreateEntry(
            identifier: "InvalidColor",
            default_value: "171,18,0",
            display_name: "Invalid Placement Color",
            description: "The color the placement preview should take on when it is invalid.\nFormat is \"R,G,B\". The alpha channel is determined by the source material.",
            is_hidden: false
            );

        SubscribeToConfigChanges(ColorPreferences, new[] { "ValidColor", "InvalidColor" }, new LemonAction<object, object>[] { ValidColorChanged, InvalidColorChanged});

        // == Rules ==
        // Likely server & client side
        SettingPreferences.CreateEntry(
            identifier: "AllowClipping",
            default_value: true,
            display_name: "Allow Clipping",
            description: "Ignore \"BLOCKED\" placement failure and turn the placement \"almost valid\" when doing so."
            );

        SettingPreferences.CreateEntry(
            identifier: "SlopedPlacementAngles",
            default_value: true,
            display_name: "Allow sloped Placement Angles",
            description: "Allows placement of gadgets at any angle up to (but not including) 90°."
            );

        // Client-side (unless the validity state of a placement is handled on each client seperately)
        SettingPreferences.CreateEntry(
            identifier: "IgnorePlayerGrounded",
            default_value: true,
            display_name: "Ignore \"MUST STAND ON GROUND\" Requirement",
            description: "Whether to allow gadgets to be placed while the player isn't grounded."
            );

        SettingPreferences.CreateEntry(
            identifier: "AllowAdvancedMovement",
            default_value: true,
            display_name: "Allow Advanced Placement Movement",
            description: "Whether to enable the advanced movement keybinds. (locking a placement into place, nudging a placement, snapping a placement to the floor)"
            );

        SettingPreferences.CreateEntry(
            identifier: "NudgeIncrement",
            default_value: 0.5f,
            display_name: "Incremental Nudge Step Size",
            description: "When Smooth Nudging is OFF: the step size of the incremental nudge.");

        SettingPreferences.CreateEntry(
            identifier: "NudgeSpeed",
            default_value: 2f,
            display_name: "Smooth Nudge Speed",
            description: "When Smooth Nudge is ON: the speed at which the placement will move when nudging.");


        // == Keybindings ==
        // Client-side
        KeybindPreferences.CreateEntry(
            identifier: "LockPlacementBind",
            default_value: KeyCode.H,
            display_name: "(Keybind) Lock Placement in Place",
            description: "Keybind for locking a placement in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeUpBind",
            default_value: KeyCode.PageUp,
            display_name: "(Keybind) Nudge Placement Upwards",
            description: "Keybind for nudging a placement upwards while it is locked in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeDownBind",
            default_value: KeyCode.PageDown,
            display_name: "(Keybind) Nudge Placement Downwards",
            description: "Keybind for nudging a placement downwards while it is locked in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeForwardBind",
            default_value: KeyCode.UpArrow,
            display_name: "(Keybind) Nudge Placement Forwards",
            description: "Keybind for nudging a placement forwards (from the player's perspective) while it is locked in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeLeftBind",
            default_value: KeyCode.LeftArrow,
            display_name: "(Keybind) Nudge Placement Left",
            description: "Keybind for nudging a placement left (from the player's perspective) while it is locked in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeRightBind",
            default_value: KeyCode.RightArrow,
            display_name: "(Keybind) Nudge Placement Right",
            description: "Keybind for nudging a placement right (from the player's perspective) while it is locked in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeBackwardBind",
            default_value: KeyCode.DownArrow,
            display_name: "(Keybind) Nudge Placement Backwards",
            description: "Keybind for nudging a placement backwards (from the player's perspective) while it is locked in place."
            );

        KeybindPreferences.CreateEntry(
            identifier: "SmoothNudgeBind",
            default_value: KeyCode.LeftAlt,
            display_name: "(Keybind) Smooth Nudge",
            description: "The keybind that must be held for smooth nudging.");

        KeybindPreferences.CreateEntry(
            identifier: "ResetBind",
            default_value: KeyCode.Delete,
            display_name: "(Keybind) Reset Placement",
            description: "Keybind for snapping a placement back to its original position after nudging it."
            );


    }
    // The methods you pass must have the following outline:
    // void Method(object OldValue, object NewValue)
    // You will have to cast them to the correct type from within the method.
    private static void SubscribeToConfigChanges(MelonPreferences_Category Category, string[] Identifiers, LemonAction<object,object>[] Methods)
    {
        if (Identifiers.Length != Methods.Length) 
            throw new ArgumentException();
        
        for(int i = 0; i < Identifiers.Length; i++)
            Category.GetEntry(Identifiers[i]).OnEntryValueChangedUntyped.Subscribe(Methods[i]);
    }

    // Delegates that are called when a config is changed
    // ==========
    private static void ValidColorChanged(object OldValue, object NewValue)
    {
        PatchHelper.SetGadgetPlacementColor(ValidColor);
    }
    private static void InvalidColorChanged(object OldValue, object NewValue)
    {
        PatchHelper.SetGadgetPlacementColor(InvalidColor, bUseInvalidColor: true);
    }

    // Helpers
    // ==========
    // Parses a color from a string with format 'R,G,B'. If the color could not be parsed, the Fallback parameter is returned.
    internal static Color ParseColor(string value, int a = 255, Color Fallback = default)
    {
        string[] Components = value.Split(',');

        if (Components.Length != 3)
            return Fallback;

        if (!int.TryParse(Components[0], out int r) || !int.TryParse(Components[1], out int g) || !int.TryParse(Components[2], out int b))
            return Fallback;

        return new Color(
            Math.Clamp((float)r / 255f, 0f, 1f),
            Math.Clamp((float)g / 255f, 0f, 1f),
            Math.Clamp((float)b / 255f, 0f, 1f),
            Math.Clamp((float)a / 255f, 0f, 1f));
    }
    // Sets the config value 'key' within 'Category' to 'value'.
    internal static void SetConfigValue<T>(MelonPreferences_Category Category, string key, T value)
    {
        Category.GetEntry<T>(key).Value = value;
        MelonPreferences.SaveCategory<MelonPreferences_Category>(Category.Identifier);
    }
}