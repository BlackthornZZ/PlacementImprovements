#define WITH_INPUT_LEGEND_MODIFICATION

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Starlight.Utils;
using UnityEngine;
using MelonLoader;
using UnityEngine.Localization;
using Il2CppSystem.Dynamic.Utils;
using UnityEngine.InputSystem;
using Il2Cpp;
using Il2CppSystem.Linq;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;
using Il2CppMonomiPark.SlimeRancher.Tutorial;
using Il2CppMonomiPark.SlimeRancher.UI.Popup;
using Il2CppMonomiPark.SlimeRancher.Util.Extensions;

namespace BuildImprovements.UI;

// Handler class for everything to do with UI in placement improvements.
internal static class AdditiveUIDirector
{
    internal static TutorialDefinition? _AdvancedMovementTutorial = null;
    public static TutorialDefinition AdvancedMovementTutorial 
    { 
        get
        {
            if (_AdvancedMovementTutorial == null)
                _AdvancedMovementTutorial = MakeAdvancedMovementTutorial();
            return _AdvancedMovementTutorial;
        }
    }

    internal static TutorialDefinition MakeAdvancedMovementTutorial()
    {
        TutorialDefinition Tutorial = new TutorialDefinition();
        Tutorial.TitleText = LanguageEUtil.AddTranslation("Advanced Gadget Movement");
        Tutorial.BodyText = LanguageEUtil.AddTranslation("You locked a gadget in place. You can now walk around it without it moving or rotating!\nTry nudging it around! You can nudge in increments or smoothly over time, vertically or horizontally.");
        Tutorial.AutoComplete = true;
        Tutorial.CheckFinishOnStart = false;
        Tutorial.CompletionUITime = 10f;
        Tutorial.OverrideCompletionUITime = true;
#if DEBUG
        Tutorial.AllowReplay = true;
#else
        Tutorial.AllowReplay = false;
#endif
        Tutorial.CompletionEvent = new();
        List<TutorialDefinition.TutorialControlLine> Controls = new();

        TutorialDefinition.TutorialControlLine Control = new();
        Control.Description = LanguageEUtil.AddTranslation("Nudge Vertically");
        Controls.Add(Control);

        Control = new();
        Control.Description = LanguageEUtil.AddTranslation("Nudge Horizontally");
        Controls.Add(Control);

        Control = new();
        Control.Description = LanguageEUtil.AddTranslation("Smooth Nudge");
        Controls.Add(Control);

        Control = new();
        Control.Description = LanguageEUtil.AddTranslation("Snap To Floor");
        Controls.Add(Control);

        return Tutorial;
    }

    public static void PlayAdvancedMovementTutorial()
    {
        //SceneContext.Instance.TutorialDirector.OpenTutorialPopup(AdvancedMovementTutorial);
        TutorialDirector Director = SceneContext.Instance.TutorialDirector;
        Director._currPopup = TutorialPopupUI.CreateTutorialPopup(AdvancedMovementTutorial).SRGetComponent<TutorialPopupUI>();
        Director._currPopup.CloseAfter(AdvancedMovementTutorial.CompletionUITime, true, true);
    }

#if WITH_INPUT_LEGEND_MODIFICATION
    public static void ModifyBottomInputLegendUI(InputLegend BottomInputLegend)
    {
        AddNewKeybindToInputLegend(BottomInputLegend, "Smooth Nudge", Key.LeftAlt);
        //AddNewKeybindToInputLegend(BottomInputLegend, "Nudge Up/Down", new[] { PreferenceDirector.NudgeUpBind, PreferenceDirector.NudgeDownBind });
        //AddHintToLegend(BottomInputLegend, "Nudge Up/Down", PlacementInputDirector.NudgeEventBinding);
    }
    public static void AddHintToLegend(InputLegend Legend, string Label, InputEventBinding EventBinding,
        string? LabelTranslationKey = null,
        string LabelTranslationTable = "Actor")
    {
        InputHintConfiguration Config = new InputHintConfiguration();

        Config.Label = LanguageEUtil.AddTranslation(Label, LabelTranslationKey, LabelTranslationTable);
        Config.DontFireInputOnClick = true;
        Config.InputEvent = EventBinding.InputEvents[0];
        Config.AdditionalInputEvent = EventBinding.InputEvents.Length > 1 ? EventBinding.InputEvents[1] : null;
        Legend._currentHints.Add(Config);
        Legend.ConfigureHints();

        InputEventDisplay IEDisplay = Legend._hints.Last()._inputDisplay;
        Il2CppReferenceArray<InputIcon> Icons = IEDisplay._iconGroup._icons;

        MelonLogger.Msg(IEDisplay._iconGroup._icons.Count);
        foreach(var icon in IEDisplay._iconGroup._icons)
        {
            MelonLogger.Msg("IM ICONNING");
            icon.gameObject.SetActive(true);
            icon._keyText.SetText("PAGEUP");
            icon.ResizeToText();
            icon.ApplyLayoutToSelf();
        }
    }
    public static Sprite IconFromKeyCode(KeyCode Code)
    {
        // tmp
        return MenuEUtil.whitePillBg;
    }
    // This is the goal method.
    public static void AddNewKeybindToInputLegend(InputLegend LegendToAddTo, string Descriptor, UnityEngine.InputSystem.Key InputKey, bool DoublePress = false)
    {
        LocalizedString Label = LanguageEUtil.AddTranslation(Descriptor);

        InputActionMapReference ActionMapRef = GameContext.Instance.InputDirector._mainGame;
        ActionMapRef._asset.Disable();

        InputAction Action = ActionMapRef.Map.FindAction(Descriptor);

        if (Action == null)
        {
            string ControlPath = Keyboard.current[InputKey].path;

            Action = new InputAction(name: Descriptor);
            Action.AddBinding(ControlPath);
            ActionMapRef.Map.AddAction(Action.name);
            Action.Enable();
        }

        InputEventBinding EventBinding = new();

        var found = ActionMapRef._asset.FindAction(Descriptor);
        // Confirmed correct
        MelonLogger.Msg("FOUND ACTION IN ASSET (WHILE DISABLED): "+found.name);

        EventBinding.ActionInstance = Action;
        EventBinding._input = new InputActionReference();
        EventBinding._input.Set(ActionMapRef._asset, ActionMapRef.Map.name, Action.name);
        // EventBinding._input.ToInputAction() != null here.

        InputEvent InputEvent = new();
        InputEvent.name = "My Input Event Name";
        EventBinding._inputEvents = Array.Empty<InputEvent>().ToIl2CppArray();
        EventBinding.InputEvents.AddLast(InputEvent);
        EventBinding.BindInput();

        GameContext.Instance.InputDirector.InputActionController._inputBindings.AddLast(EventBinding);

        ActionMapRef._asset.Enable();

        // Confirmed correct
        MelonLogger.Msg("After re-enabling the asset, we found the following action: " + ActionMapRef._asset.FindAction(Descriptor).name);

        InputHintConfiguration TmpConfig = new();
        TmpConfig.Label = Label;
        TmpConfig.InputEvent = InputEvent;
        TmpConfig.SupportedDevices = InputHintDeviceFlag.KEYBOARD_AND_MOUSE;
        //List<InputHintConfiguration> AllInputHints = LegendToAddTo._currentHints.ToNetList();
        //AllInputHints.Add(TmpConfig);

        // this might be my worst work yet (reinterpret downcast to the IEnumerable)
        //Il2CppSystem.Collections.Generic.IEnumerable<InputHintConfiguration> enumerable = InteropStatics.ReinterpretCast<Il2CppSystem.Collections.Generic.List<InputHintConfiguration>, Il2CppSystem.Collections.Generic.IEnumerable<InputHintConfiguration>>(AllInputHints.ToIl2CppList());
        
        // Confirmed correct
        //MelonLogger.Msg("Enumerable count: "+enumerable.Count());

        //LegendToAddTo.SetInputHints(enumerable);

        // Confirmed correct
       //MelonLogger.Msg(string.Format("Length of current hints after setting of input hints: {0}, Last Hint Event.Name: {1}", LegendToAddTo._currentHints.Count, LegendToAddTo._currentHints[LegendToAddTo._currentHints.Count - 1].InputEvent.name));

        //MelonLogger.Msg(string.Format("Length of hints after setting input hints: {0}, Last Hint Name: {1}", LegendToAddTo._hints.Count, LegendToAddTo._hints.Last().GetName()));
        //LegendToAddTo.InvalidateDisplay();

        GadgetInputLegendUpdater Updater = LegendToAddTo.gameObject.GetComponent<GadgetInputLegendUpdater>();
        List<InputHintConfiguration> Hints = Updater._inputLegendConfiguration.GadgetSelectedInputLegend._hints.ToNetList();
        Hints.Add(TmpConfig);
        Updater._inputLegendConfiguration.GadgetSelectedInputLegend._hints = Hints.ToIl2CppArray();
        Updater.UpdateInputLegend();

        //InputHint ThisHint = LegendToAddTo._hints.Last();
        //ThisHint.SetEnabled(true);

        //KeyboardMouseIcon KBMIcon = new();
        //KBMIcon._control = Keyboard.current[InputKey].path;
        //MelonLogger.Msg("path is " + KBMIcon._control);
        //KBMIcon._isDoubleTap = DoublePress;

        //IInputIcon KBMAsInterface = InteropStatics.ReinterpretCast<KeyboardMouseIcon, IInputIcon>(KBMIcon);
        //ThisHint._inputDisplay._inputIcons.Add(KBMAsInterface);
        //ThisHint._inputDisplay._iconGroup.enabled = true;
        //ThisHint._inputDisplay._iconGroup.ShowMultipleInputs = false;
        //ThisHint._inputDisplay._iconGroup.SetIconEntries(ThisHint._inputDisplay._inputIcons);
        //InputIconGroup group = ThisHint._inputDisplay._iconGroup;
        //ThisHint._inputDisplay.gameObject.SetActive(true);

        //LegendToAddTo._hints.Last()._inputDisplay.ConfigureDisplay();
    }
    public static string[] GetInputHintKeys(InputLegend ParentLegend, string Descriptor) { return Array.Empty<string>(); }
    public static void SetInputHintKeys(InputLegend ParentLegend, string ExistingDescriptor, KeyCode[] NewKeys) { }
#endif
}
