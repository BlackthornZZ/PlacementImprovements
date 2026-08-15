using BuildImprovements.Injected;
using BuildImprovements.Preferences;
using BuildImprovements.UI;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Player;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;
using Il2CppMonomiPark.SlimeRancher.Util.Extensions;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppSystem;
using MelonLoader;
using Starlight.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace BuildImprovements.Input;
public class PlacementInputDirector
{
    internal static readonly LocalizedString MaxNudgeString = LanguageEUtil.AddTranslation("Max Nudge Distance Reached");
    internal static readonly LocalizedString EyedropperNoneAvailableString = LanguageEUtil.AddTranslation("None In Storage");
    internal static readonly LocalizedString EyedropperNoTarget = LanguageEUtil.AddTranslation("Not a Gadget");
    internal float LastNudgeWarning = 0f;
    internal const float NudgeWarningInterval = 1f;

    internal Vector3 LockedPlacementPosition = Vector3.zero;
    internal Vector3 InitialLockedPlacementPosition = Vector3.zero;
    internal Quaternion LockedPlacementRotation = Quaternion.identity;
    internal bool bPlacementLocked = false;
    public void OnGadgetSelected(GadgetItem GItem) => ResetLock(GItem, false);
    public void OnInputDirectorUpdate(GadgetItem GItem) => CheckInputs(GItem, SceneContext.Instance.PlayerState.GadgetModeActive);
    public void OnPostGadgetItemFootprintUpdate(GadgetItem GItem) 
    {
        // This should not happen!!!
        if ((!GItem._gadgetFootprintInstance || !GItem._gadgetPlaceholderInstance) && bPlacementLocked)
            bPlacementLocked = false;

        if(bPlacementLocked)
        {
            SetLockedTransform(GItem);
            GItem._gadgetPlaceholderInstance.SetActive(true);
            GItem._gadgetFootprintInstance.SetActive(true);

            if (GItem._rotatingClockwise)
                LockedPlacementRotation *= Quaternion.Euler(0f, GItem.GadgetItemMetadata.GadgetRotationSpeed * Time.deltaTime, 0f);
            if (GItem._rotatingCounterClockwise)
                LockedPlacementRotation *= Quaternion.Euler(0f, -GItem.GadgetItemMetadata.GadgetRotationSpeed * Time.deltaTime, 0f);
        }
    }

    public void SetLockedTransform(GadgetItem GItem)
    {
        GItem._gadgetFootprintInstance.transform.SetPositionAndRotation(LockedPlacementPosition, LockedPlacementRotation);
        GItem._gadgetPlaceholderInstance.transform.SetPositionAndRotation(LockedPlacementPosition, LockedPlacementRotation);
    }

    public void CheckInputs(GadgetItem GItem, bool bGadgetMode)
    {
        if (!bGadgetMode || !bPlacementLocked) return;

        DoNudge(GItem);
    }
    public void OnGadgetCleared(GadgetItem GItem) => ResetLock(GItem, false);

    public void ResetLock(GadgetItem GItem, bool bPreservePlacementRotation = false)
    {
        bPlacementLocked = false;
        if (bPreservePlacementRotation && GItem._gadgetPlaceholderInstance && GItem._gadgetFootprintInstance)
        {
            // It feels strange if its not rotated accordingly
            GItem._gadgetPlaceholderInstance.transform.rotation = LockedPlacementRotation;
            GItem._gadgetFootprintInstance.transform.rotation = LockedPlacementRotation;
            GItem._gadgetRotation = LockedPlacementRotation;
        }
        LockedPlacementPosition = Vector3.zero;
        InitialLockedPlacementPosition = Vector3.zero;
        LockedPlacementRotation = Quaternion.identity;

     
    }

    public void DoNudge(GadgetItem GItem)
    {
        Vector3 NudgeDelta = Vector3.zero;
        float NudgeDeltaMultiplier = PreferenceDirector.bSmoothNudge ? PreferenceDirector.NudgeSpeed * Time.deltaTime : PreferenceDirector.NudgeIncrementScale;

        // Up / Down
        if (CheckNudgeKey(PreferenceDirector.NudgeUpBind))
            NudgeDelta.y += 1;
        if (CheckNudgeKey(PreferenceDirector.NudgeDownBind))
            NudgeDelta.y -= 1;

        // Forward / Back
        if (CheckNudgeKey(PreferenceDirector.NudgeForwardBind))
            NudgeDelta += SceneContext.Instance.player.transform.rotation * Vector3.forward;
        if (CheckNudgeKey(PreferenceDirector.NudgeBackwardBind))
            NudgeDelta += SceneContext.Instance.player.transform.rotation * Vector3.back;

        // Left / Right
        if (CheckNudgeKey(PreferenceDirector.NudgeLeftBind))
            NudgeDelta += SceneContext.Instance.player.transform.rotation * Vector3.left;
        if (CheckNudgeKey(PreferenceDirector.NudgeRightBind))
            NudgeDelta += SceneContext.Instance.player.transform.rotation * Vector3.right;

        NudgeDelta *= NudgeDeltaMultiplier;

        if (NudgeDelta.sqrMagnitude <= 0) return;

        //SceneContext.Instance.eventDirector.RaiseEvent(NudgeGameEventQueryComponent.OnNudgedEvent);

        if((InitialLockedPlacementPosition - (LockedPlacementPosition + NudgeDelta)).sqrMagnitude >= 100)
        {
            if (!PreferenceDirector.bSmoothNudge || Time.timeSinceLevelLoad - LastNudgeWarning >= NudgeWarningInterval)
            {
                if (PreferenceDirector.bSmoothNudge)
                    LastNudgeWarning = Time.timeSinceLevelLoad;

                GItem.PlayTransientAudio(GItem.GadgetItemMetadata.BlockedPlacementErrorCue);
                HudUI.Instance.FlashErrorMessage(MaxNudgeString);
            }

            return;
        }
        LockedPlacementPosition += NudgeDelta;
    }

    public void DoGadgetEyedropper(GadgetItem GItem)
    {
        if (GItem._player.GadgetModeActive.Value ? !GItem._gadgetDirector.TargetedGadget.Value : (TargetingUI.Instance.GetTargetObject() == null || !TargetingUI.Instance.GetGadgetTargetInfo(TargetingUI.Instance.GetTargetObject())))
        {
            GItem.PlayTransientAudio(GItem.GadgetItemMetadata.BlockedPlacementErrorCue);
            HudUI.Instance.FlashErrorMessage(EyedropperNoTarget);
            return;
        }

        GadgetDefinition TargetedDefinition = GItem._player.GadgetModeActive.Value ? GItem._gadgetDirector.TargetedGadget.Value.IdentTypeAsDefinition : TargetingUI.Instance.GetTargetObject().SRGetComponentInParent<Gadget>().IdentTypeAsDefinition;

        if(GItem._gadgetDirector._model.GetCount(TargetedDefinition) <= 0)
        {
            GItem.PlayTransientAudio(GItem.GadgetItemMetadata.BlockedPlacementErrorCue);
            HudUI.Instance.FlashErrorMessage(EyedropperNoneAvailableString);
            return;
        }

        if (!GItem._player.GadgetModeActive)
        {
            GItem._player.GadgetModeActive.Set(true);
            GItem._player.VacuumItem.OnItemDeactivated();
            GItem.OnItemActivated();
        }
        GItem._gadgetDirector._model.TrySelectGadget(TargetedDefinition);
        GItem._gadgetDirector.UpdateSelectedGadget();
        GItem.SetHeldGadget(TargetedDefinition);

    }
    internal static bool CheckNudgeKey(KeyCode inKey) => PreferenceDirector.bSmoothNudge ? InputEUtil.OnKey(inKey) : InputEUtil.OnKeyDown(inKey);
    public void SetPlacementLocked(GadgetItem GItem, bool bNewPlacementLocked)
    {
        // Can't set placement locked if there's no instance to lock!
        if(!GItem._gadgetPlaceholderInstance)
            return;

        bPlacementLocked = bNewPlacementLocked;
        HudUI.Instance.BottomInputLegend.gameObject.GetComponent<GadgetInputLegendUpdater>().UpdateInputLegend();

        if (bNewPlacementLocked)
        {
            LockedPlacementPosition = GItem._gadgetPlaceholderInstance.transform.position;
            InitialLockedPlacementPosition = LockedPlacementPosition;
            LockedPlacementRotation = GItem._gadgetPlaceholderInstance.transform.rotation;

            if(SceneContext.Instance.TutorialDirector._currPopup == null)
                Main.AdditiveUIDirector.TryPlayAdvancedMovementTutorial();
        }
        else ResetLock(GItem, true);
    }

    // InputEvent callbacks
    public static void GadgetLock_Performed(InputEventData Data)
    {
        if (!PreferenceDirector.bAllowAdvancedMovement) return;

        GadgetItem GItem = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem;

        if (GItem._isFootprintVisible && GItem._gadgetDirector.SelectedSlottedGadget != null)
            Main.PlacementInputDirector.SetPlacementLocked(GItem, !Main.PlacementInputDirector.bPlacementLocked);
    }
    public static void GadgetEyedropper_Performed(InputEventData Data) => Main.PlacementInputDirector.DoGadgetEyedropper(SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem);
}
