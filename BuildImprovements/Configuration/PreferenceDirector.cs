using BuildImprovements.Input;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using Starlight.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace BuildImprovements.Configuration;
public static class PreferenceDirector
{
    public static readonly MelonPreferences_Category ColorPreferences = MelonPreferences.CreateCategory("Placement Improvements - Colors"); 
    public static readonly MelonPreferences_Category SettingPreferences = MelonPreferences.CreateCategory("Placement Improvements - Settings");
    public static readonly MelonPreferences_Category KeybindPreferences = MelonPreferences.CreateCategory("Placement Improvements - Keybinds");

    // Enabled/disabled flags for individual build improvements.
    public static PreferenceEntry AllowSlopedPlacementAngle => GlobalConfiguration["SlopedPlacementAngles"];
    public static PreferenceEntry IgnorePlayerGroundedState => GlobalConfiguration["IgnorePlayerGrounded"];
    public static PreferenceEntry AllowClipping => GlobalConfiguration["Clipping"];
    public static PreferenceEntry AllowAdvancedMovement => GlobalConfiguration["AdvancedMovement"];
    public static PreferenceEntry AllowPrismacoreGadgets => GlobalConfiguration["PrismacoreGadgets"];
    public static PreferenceEntry AllowFloatingGadgets => GlobalConfiguration["FloatingGadgets"];
    public static PreferenceEntry InfiniteLinkedRange => GlobalConfiguration["InfiniteLinkedRange"];
    public static PreferenceEntry InvertSmoothNudge => GlobalConfiguration["InvertedSmoothNudge"];

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
    internal static bool bGadgetEyedropperMiddleClick
    {
        get => KeybindPreferences.GetEntry<bool>("GadgetEyedropperMiddleClick").Value;
        private set => SetConfigValue<bool>(KeybindPreferences, "GadgetEyedropperMiddleClick", value);
    }
    internal static KeyCode GadgetEyedropperBind 
    {
        get => KeybindPreferences.GetEntry<KeyCode>("GadgetEyedropperBind").Value;
        private set => SetConfigValue<KeyCode>(KeybindPreferences, "GadgetEyedropperBind", value);
    }

    // Nudging features
    internal static bool SmoothNudge => InvertSmoothNudge ^ InputEUtil.OnKey(SmoothNudgeBind);
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

    // Configuration API
    internal static readonly HashSet<Configurator> ExternalConfigurators = new();
    internal static readonly Dictionary<string, PreferenceEntry> GlobalConfiguration = new();

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

        SubscribeToConfigChanges(ColorPreferences, new[] { "ValidColor", "InvalidColor" }, new LemonAction<object, object>[] { ValidColorChanged, InvalidColorChanged });

        // == Rules ==
        CreateConfigurableBoolean("Clipping", true, "Allow Clipping", "Ignore \"BLOCKED\" placement failure and turn the placement \"almost valid\" when doing so.");
        CreateConfigurableBoolean("SlopedPlacementAngles", true, "Allow sloped Placement Angles", "Allows placement of gadgets at any angle.");
        CreateConfigurableBoolean("IgnorePlayerGrounded", true, "Ignore \"MUST STAND ON GROUND\" Requirement");
        CreateConfigurableBoolean("AdvancedMovement", true, "Allow Advanced Placement Movement", "Whether to enable the advanced movement keybinds.");
        CreateConfigurableBoolean("PrismacoreGadgets", true, "Prismacore Gadget Mode", "Whether to allow building gadgets in the Prismacore (when it is harmonized).");
        CreateConfigurableBoolean("FloatingGadgets", true, "Allow Floating Gadgets", "Note: very unforgiving when off!");
        CreateConfigurableBoolean("InfiniteLinkedRange", false, "Infinite Linked Gadget Range");
        CreateConfigurableBoolean("InvertedSmoothNudge", false, "Invert Smooth Nudge", "When this is turned on, smooth nudging will be the default, while holding its key results in the incremental nudge being enabled.");

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

        SettingPreferences.GetEntry("AdvancedMovement").OnEntryValueChangedUntyped.Subscribe(AdvancedMovementChanged);
        SettingPreferences.GetEntry("InvertedSmoothNudge").OnEntryValueChangedUntyped.Subscribe(InvertSmoothNudgeChanged);

        // == Keybindings ==
        // Client-side
        KeybindPreferences.CreateEntry(
            identifier: "LockPlacementBind",
            default_value: KeyCode.H,
            display_name: "(Keybind) Lock Placement in Place"
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeUpBind",
            default_value: KeyCode.PageUp,
            display_name: "(Keybind) Nudge Placement Upwards"
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeDownBind",
            default_value: KeyCode.PageDown,
            display_name: "(Keybind) Nudge Placement Downwards"
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeForwardBind",
            default_value: KeyCode.UpArrow,
            display_name: "(Keybind) Nudge Placement Forwards"
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeLeftBind",
            default_value: KeyCode.LeftArrow,
            display_name: "(Keybind) Nudge Placement Left"
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeRightBind",
            default_value: KeyCode.RightArrow,
            display_name: "(Keybind) Nudge Placement Right"
            );

        KeybindPreferences.CreateEntry(
            identifier: "NudgeBackwardBind",
            default_value: KeyCode.DownArrow,
            display_name: "(Keybind) Nudge Placement Backwards"
            );

        KeybindPreferences.CreateEntry(
            identifier: "SmoothNudgeBind",
            default_value: KeyCode.LeftAlt,
            display_name: "(Keybind) Smooth Nudge",
            description: "The keybind that must be <b>held</b> for smooth nudging.");

        KeybindPreferences.CreateEntry(
            identifier: "GadgetEyedropperMiddleClick",
            default_value: true,
            display_name: "Use Middle Click For Eyedropper",
            description: "Whether to use the mouse middle click for the eyedropper. If unchecked, the keybind is used."
            );

        KeybindPreferences.CreateEntry(
            identifier: "GadgetEyedropperBind",
            default_value: KeyCode.I,
            display_name: "(Keybind) Gadget Eyedropper"
            );

        SubscribeToConfigChanges(KeybindPreferences, AnyKeybindChanged, "LockPlacementBind", "NudgeUpBind", "NudgeDownBind", "NudgeForwardBind", "NudgeLeftBind", "NudgeRightBind", "NudgeBackwardBind", "SmoothNudgeBind", "GadgetEyedropperMiddleClick", "GadgetEyedropperBind");
    }
    private static PreferenceEntry CreateConfigurableBoolean(string Identifier, bool DefaultValue, string? DisplayName = null, string? Description = null, bool IsHidden = false, bool DontSaveDefault = false, ValueValidator? Validator = null)
    {
        MelonPreferences_Entry<bool> MelonEntry = SettingPreferences.CreateEntry<bool>(Identifier, DefaultValue, DisplayName, Description, IsHidden, DontSaveDefault, Validator);
        PreferenceEntry Entry = new PreferenceEntry(MelonEntry);
        GlobalConfiguration.Add(Identifier, Entry);
        return Entry;
    }

    internal static void ApplyExternalConfigurators()
    {
        List<Configurator> ConfiguratorsByPriority = ExternalConfigurators.ToList();
        ConfiguratorsByPriority.Sort((a, b) => a.CompareTo(b));

        foreach (Configurator ExternalConfig in ConfiguratorsByPriority)
        {
            MelonLogger.Msg("Applying External Configurator (Source: {0}) with {1} override(s)", ExternalConfig.Source, ExternalConfig.ConfigurationOverrides.Count);
            foreach (KeyValuePair<string, bool> Override in ExternalConfig.ConfigurationOverrides)
            {
                if(!GlobalConfiguration.ContainsKey(Override.Key))
                {
                    MelonLogger.Warning("External configurator (Source: {0}) attemped to change a config (\"{1}\", with value {2}) that doesn't exist or can't be externally configured. Not all Melon preference entries can be configured.", ExternalConfig.Source, Override.Key, Override.Value);
                    continue;
                }
                GlobalConfiguration[Override.Key].ApplyWithFlags(ExternalConfig.ConfiguratorFlags, Override.Value);
            }
        }
    }

    internal static void UpdateGlobalConfiguration()
    {
        foreach (KeyValuePair<string, PreferenceEntry> Config in GlobalConfiguration)
        {
            Config.Value.ClearOverride();
        }

        ApplyExternalConfigurators();
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

    // Link all identifiers to one method.
    private static void SubscribeToConfigChanges(MelonPreferences_Category Category, LemonAction<object, object> Method, params string[] Identifiers)
    {
        foreach(string ID in Identifiers)
            Category.GetEntry(ID).OnEntryValueChangedUntyped.Subscribe(Method);
    }

    // Delegates that are called when a config is changed
    // ==========
    private static void ValidColorChanged(object OldValue, object NewValue)
    {
        Main.PatchHelper.SetGadgetPlacementColor(ValidColor);
    }
    private static void InvalidColorChanged(object OldValue, object NewValue)
    {
        Main.PatchHelper.SetGadgetPlacementColor(InvalidColor, bUseInvalidColor: true);
    }
    private static void AnyKeybindChanged(object OldValue, object NewValue)
    {
        GameContext.Instance.InputDirector._mainGame.Map.asset.Disable();
        InputRegistrar.ResetPlacementImprovementsInputs();
        GameContext.Instance.InputDirector._mainGame.Map.asset.Enable();
    }

    private static void AdvancedMovementChanged(object OldValue, object NewValue)
        => Main.AdditiveUIDirector.ChangeGadgetLockInputHints((bool)NewValue);
    private static void InvertSmoothNudgeChanged(object OldValue, object NewValue)
        => Main.AdditiveUIDirector.ChangeSmoothNudgeLabel((bool)NewValue);

    // Helpers
    // ==========
    // Parses a color from a string with format 'R,G,B'. If the color could not be parsed, the Fallback parameter is returned.
    private static Color ParseColor(string value, int a = 255, Color Fallback = default)
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
    private static void SetConfigValue<T>(MelonPreferences_Category Category, string key, T value)
    {
        Category.GetEntry<T>(key).Value = value;
        MelonPreferences.SaveCategory<MelonPreferences_Category>(Category.Identifier);
    }
}