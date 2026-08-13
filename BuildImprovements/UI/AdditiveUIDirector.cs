
using BuildImprovements.Input;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.Tutorial;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;
using Il2CppMonomiPark.SlimeRancher.UI.Popup;
using Il2CppMonomiPark.SlimeRancher.Util.Extensions;
using Il2CppSystem.Dynamic.Utils;
using Il2CppSystem.Linq;
using MelonLoader;
using Starlight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace BuildImprovements.UI;

// Handler class for everything to do with UI in placement improvements.
internal static class AdditiveUIDirector
{
    internal static TutorialDefinition? _AdvancedMovementTutorial = null;
    internal static InputLegendConfiguration? GadgetLockedInputLegend = null;
    internal static bool bInputLegendsModified = false;
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
        TutorialDirector Director = SceneContext.Instance.TutorialDirector;
        Director._currPopup = TutorialPopupUI.CreateTutorialPopup(AdvancedMovementTutorial).SRGetComponent<TutorialPopupUI>();
        Director._currPopup.CloseAfter(AdvancedMovementTutorial.CompletionUITime, true, true);
    }

    public static void ModifyBottomInputLegendUI(InputLegend BottomInputLegend)
    {
        if (bInputLegendsModified) return;

        GadgetItemMetadata GadgetItemData = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem.GadgetItemMetadata;
        LocalizedString CopyGadgetLabel = LanguageEUtil.AddTranslation("Copy");
        LocalizedString NudgeLabel = LanguageEUtil.AddTranslation("Nudge");
        GadgetInputLegendConfiguration GadgetInputLegendConfig = BottomInputLegend.gameObject.GetComponent<GadgetInputLegendUpdater>()._inputLegendConfiguration;

        GadgetLockedInputLegend = UnityEngine.Object.Instantiate(GadgetInputLegendConfig.GadgetSelectedInputLegend);

        ChangeInputHintLabel(GadgetLockedInputLegend, GadgetItemData.StoreGadget, "Unlock");
        // "Gadget Inventory" input event. Couldn't find where it was stored but this works fine.
        RemoveInputHint(GadgetLockedInputLegend, GadgetInputLegendConfig.NoSelectionOrTargetInputLegend._hints.First().InputEvent);
        AddNewKeybindToInputLegend(GadgetLockedInputLegend, "Smooth", InputRegistrar.EventStore.SmoothNudge);
        AddNewKeybindToInputLegend(GadgetLockedInputLegend, NudgeLabel, InputRegistrar.EventStore.NudgeUpDown);
        AddNewKeybindToInputLegend(GadgetLockedInputLegend, NudgeLabel, InputRegistrar.EventStore.NudgeForwardBack, InputRegistrar.EventStore.NudgeLeftRight);

        AddNewKeybindToInputLegend(GadgetInputLegendConfig.GadgetSelectedInputLegend, "Lock", InputRegistrar.EventStore.GadgetLock);
        InsertKeybindAfter(GadgetInputLegendConfig.GadgetSelectedInputLegend, GadgetItemData.PlaceGadget, CopyGadgetLabel, InputRegistrar.EventStore.GadgetEyedrop);
        InsertKeybindAfter(GadgetInputLegendConfig.GadgetTargetedInputLegend, GadgetItemData.PlaceGadget, CopyGadgetLabel, InputRegistrar.EventStore.GadgetEyedrop);

        BottomInputLegend.Configure(GadgetInputLegendConfig.GadgetSelectedInputLegend);
        BottomInputLegend.InvalidateDisplay();

        bInputLegendsModified = true;
    }
    public static void AddNewKeybindToInputLegend(InputLegendConfiguration LegendConfig, string Descriptor, InputEvent Event, InputEvent? AdditionalInputEvent = null) => 
        AddNewKeybindToInputLegend(LegendConfig,LanguageEUtil.AddTranslation(Descriptor), Event, AdditionalInputEvent);
    public static void AddNewKeybindToInputLegend(InputLegendConfiguration LegendConfig, LocalizedString Label, InputEvent Event, InputEvent? AdditionalInputEvent = null)
    {
        InputHintConfiguration config = new()
        {
            InputEvent = Event,
            AdditionalInputEvent = AdditionalInputEvent,
            Label = Label
        };

        List<InputHintConfiguration> Hints = LegendConfig._hints.ToNetList();
        Hints.Add(config);
        LegendConfig._hints = Hints.ToIl2CppArray();
    }
    public static void InsertKeybindAfter(InputLegendConfiguration LegendConfig, InputEvent After, LocalizedString Label, InputEvent Event, InputEvent? AdditionalInputEvent = null)
    {
        InputHintConfiguration? AfterHint = LegendConfig._hints.FirstOrDefault(hint => hint.InputEvent == After || hint.AdditionalInputEvent == After);
        if(AfterHint == null)
        {
            MelonLogger.Warning("InsertKeybindAfter was called, but the After event wasn't present in the passed configuration!");
            return;
        }

        int AfterIndex = LegendConfig._hints.IndexOf(AfterHint);

        InputHintConfiguration config = new()
        {
            InputEvent = Event,
            AdditionalInputEvent = AdditionalInputEvent,
            Label = Label
        };

        List<InputHintConfiguration> Hints = LegendConfig._hints.ToNetList();
        Hints.Insert(AfterIndex + 1, config);
        LegendConfig._hints = Hints.ToIl2CppArray();
    }
    public static LocalizedString? ChangeInputHintLabel(InputLegendConfiguration Config, InputEvent TargetEvent, string NewDescriptor) => 
        ChangeInputHintLabel(Config, TargetEvent, LanguageEUtil.AddTranslation(NewDescriptor));
    public static LocalizedString? ChangeInputHintLabel(InputLegendConfiguration Config, InputEvent TargetEvent, LocalizedString NewLabel)
    {
        InputHintConfiguration? HintConfig = Config._hints.FirstOrDefault(hint => hint.InputEvent == TargetEvent || hint.AdditionalInputEvent == TargetEvent);
        if (HintConfig == null)
        {
            MelonLogger.Warning("ChangeInputHintLabel was called but the specified InputEvent couldnt be found!");
            return null;
        }
        LocalizedString OldLabel = HintConfig.Label;
        HintConfig.Label = NewLabel;

        return OldLabel;
    }
    public static void RemoveInputHint(InputLegendConfiguration Config, InputEvent TargetEvent)
    {
        InputHintConfiguration? HintConfig = Config._hints.FirstOrDefault(hint => hint.InputEvent == TargetEvent || hint.AdditionalInputEvent == TargetEvent);
        if (HintConfig == null)
        {
            MelonLogger.Warning("RemoveInputHint was called but the specified InputEvent couldnt be found!");
            return;
        }

        Config._hints = Config._hints.RemoveAtToNew(Config._hints.IndexOf(HintConfig));
    }
}
