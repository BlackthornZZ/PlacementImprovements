using BuildImprovements.Injected;
using BuildImprovements.Preferences;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;
using Il2CppMonomiPark.SlimeRancher.Util.Extensions;
using Il2CppMonomiPark.SlimeRancher.World;
using Starlight.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
    private readonly GadgetItem PlayerGadgetItem;

    public PlacementInputDirector()
    {
        PlayerGadgetItem = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem;
    }

    public void OnGadgetSelected() => ResetLock(false);
    public void OnInputDirectorUpdate() => CheckInputs(SceneContext.Instance.PlayerState.GadgetModeActive);
    public void OnPostGadgetItemFootprintUpdate() 
    {
        // This should not happen!!!
        if ((!PlayerGadgetItem._gadgetFootprintInstance || !PlayerGadgetItem._gadgetPlaceholderInstance) && bPlacementLocked)
            bPlacementLocked = false;

        if(bPlacementLocked)
        {
            SetLockedTransform();
            PlayerGadgetItem._gadgetPlaceholderInstance.SetActive(true);
            PlayerGadgetItem._gadgetFootprintInstance.SetActive(true);

            if (PlayerGadgetItem._rotatingClockwise)
                LockedPlacementRotation *= Quaternion.Euler(0f, PlayerGadgetItem.GadgetItemMetadata.GadgetRotationSpeed * Time.deltaTime, 0f);
            if (PlayerGadgetItem._rotatingCounterClockwise)
                LockedPlacementRotation *= Quaternion.Euler(0f, -PlayerGadgetItem.GadgetItemMetadata.GadgetRotationSpeed * Time.deltaTime, 0f);
        }
    }

    public void SetLockedTransform()
    {
        PlayerGadgetItem._gadgetFootprintInstance.transform.SetPositionAndRotation(LockedPlacementPosition, LockedPlacementRotation);
        PlayerGadgetItem._gadgetPlaceholderInstance.transform.SetPositionAndRotation(LockedPlacementPosition, LockedPlacementRotation);
    }

    public void CheckInputs(bool bGadgetMode)
    {
        if (!bGadgetMode || !bPlacementLocked) return;

        DoNudge();
    }
    public void OnGadgetCleared() => ResetLock(false);

    public void ResetLock(bool bPreservePlacementRotation = false)
    {
        bPlacementLocked = false;
        if (bPreservePlacementRotation && PlayerGadgetItem._gadgetPlaceholderInstance && PlayerGadgetItem._gadgetFootprintInstance)
        {
            // It feels strange if its not rotated accordingly
            PlayerGadgetItem._gadgetPlaceholderInstance.transform.rotation = LockedPlacementRotation;
            PlayerGadgetItem._gadgetFootprintInstance.transform.rotation = LockedPlacementRotation;
            PlayerGadgetItem._gadgetRotation = LockedPlacementRotation;
        }
        LockedPlacementPosition = Vector3.zero;
        InitialLockedPlacementPosition = Vector3.zero;
        LockedPlacementRotation = Quaternion.identity;
    }

    public void DoNudge()
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

                PlayerGadgetItem.PlayTransientAudio(PlayerGadgetItem.GadgetItemMetadata.BlockedPlacementErrorCue);
                HudUI.Instance.FlashErrorMessage(MaxNudgeString);
            }

            return;
        }
        LockedPlacementPosition += NudgeDelta;
    }

    public void DoGadgetEyedropper()
    {
        if (PlayerGadgetItem._player.GadgetModeActive.Value ? !PlayerGadgetItem._gadgetDirector.TargetedGadget.Value : (TargetingUI.Instance.GetTargetObject() == null || !TargetingUI.Instance.GetGadgetTargetInfo(TargetingUI.Instance.GetTargetObject())))
        {
            PlayerGadgetItem.PlayTransientAudio(PlayerGadgetItem.GadgetItemMetadata.BlockedPlacementErrorCue);
            HudUI.Instance.FlashErrorMessage(EyedropperNoTarget);
            return;
        }

        GadgetDefinition TargetedDefinition = PlayerGadgetItem._player.GadgetModeActive.Value ? PlayerGadgetItem._gadgetDirector.TargetedGadget.Value.IdentTypeAsDefinition : TargetingUI.Instance.GetTargetObject().SRGetComponentInParent<Gadget>().IdentTypeAsDefinition;

        if(PlayerGadgetItem._gadgetDirector._model.GetCount(TargetedDefinition) <= 0)
        {
            PlayerGadgetItem.PlayTransientAudio(PlayerGadgetItem.GadgetItemMetadata.BlockedPlacementErrorCue);
            HudUI.Instance.FlashErrorMessage(EyedropperNoneAvailableString);
            return;
        }

        if (!PlayerGadgetItem._player.GadgetModeActive)
        {
            PlayerGadgetItem._player.GadgetModeActive.Set(true);
            PlayerGadgetItem._player.VacuumItem.OnItemDeactivated();
            PlayerGadgetItem.OnItemActivated();
        }
        PlayerGadgetItem._gadgetDirector._model.TrySelectGadget(TargetedDefinition);
        PlayerGadgetItem._gadgetDirector.UpdateSelectedGadget();
        PlayerGadgetItem.SetHeldGadget(TargetedDefinition);

    }
    internal static bool CheckNudgeKey(KeyCode inKey) => PreferenceDirector.bSmoothNudge ? InputEUtil.OnKey(inKey) : InputEUtil.OnKeyDown(inKey);
    public void SetPlacementLocked(bool bNewPlacementLocked)
    {
        // Can't set placement locked if there's no instance to lock!
        if(!PlayerGadgetItem._gadgetPlaceholderInstance)
            return;

        bPlacementLocked = bNewPlacementLocked;
        HudUI.Instance.BottomInputLegend.gameObject.GetComponent<GadgetInputLegendUpdater>().UpdateInputLegend();

        if (bNewPlacementLocked)
        {
            LockedPlacementPosition = PlayerGadgetItem._gadgetPlaceholderInstance.transform.position;
            InitialLockedPlacementPosition = LockedPlacementPosition;
            LockedPlacementRotation = PlayerGadgetItem._gadgetPlaceholderInstance.transform.rotation;

            if(SceneContext.Instance.TutorialDirector._currPopup == null)
                Main.AdditiveUIDirector.TryPlayAdvancedMovementTutorial();
        }
        else ResetLock(true);
    }

    // InputEvent callbacks
    public static void GadgetLock_Performed(InputEventData Data)
    {
        if (!PreferenceDirector.bAllowAdvancedMovement) return;

        if (Main.PlacementInputDirector.PlayerGadgetItem._isFootprintVisible && SceneContext.Instance.GadgetDirector.SelectedSlottedGadget != null)
            Main.PlacementInputDirector.SetPlacementLocked(!Main.PlacementInputDirector.bPlacementLocked);
    }
    public static void GadgetEyedropper_Performed(InputEventData Data) => Main.PlacementInputDirector.DoGadgetEyedropper();
}
