
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
using BuildImprovements.Input;

namespace BuildImprovements.UI;

// Handler class for everything to do with UI in placement improvements.
internal static class AdditiveUIDirector
{
    internal static TutorialDefinition? _AdvancedMovementTutorial = null;
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

        GadgetInputLegendConfiguration GadgetInputLegendConfig = BottomInputLegend.gameObject.GetComponent<GadgetInputLegendUpdater>()._inputLegendConfiguration;

        AddNewKeybindToInputLegend(BottomInputLegend, GadgetInputLegendConfig.GadgetSelectedInputLegend, "Lock Gadget", InputRegistrar.EventStore.GadgetLock);
        AddNewKeybindToInputLegend(BottomInputLegend, GadgetInputLegendConfig.GadgetTargetedInputLegend, "Copy Gadget", InputRegistrar.EventStore.GadgetEyedrop);

        bInputLegendsModified = true;
    }
    public static void AddNewKeybindToInputLegend(InputLegend Legend, InputLegendConfiguration LegendConfig, string Descriptor, InputEvent Event, bool DoublePress = false)
    {
        LocalizedString Label = LanguageEUtil.AddTranslation(Descriptor);

        InputHintConfiguration config = new()
        {
            InputEvent = Event,
            Label = Label
        };

        List<InputHintConfiguration> Hints = LegendConfig._hints.ToNetList();
        Hints.Add(config);
        LegendConfig._hints = Hints.ToIl2CppArray();
        Legend.SetInputHints(InteropStatics.ReinterpretCast<Il2CppSystem.Collections.Generic.List<InputHintConfiguration>, Il2CppSystem.Collections.Generic.IEnumerable<InputHintConfiguration>>(Hints.ToIl2CppList()));
    }
}
