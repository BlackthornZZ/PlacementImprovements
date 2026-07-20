using BuildImprovements.Patches;
using BuildImprovements.Preferences;
using Harmony;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Il2CppSystem.Dynamic.Utils;
using MelonLoader;
using Starlight.Enums;
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
internal static class PlacementInputDirector
{
    internal static InputEventBinding NudgeEventBinding = null!;
    internal static InputEventBinding LockBinding = null!;
    internal static InputEventBinding ResetBinding = null!;
    internal static InputActionMap InputMap = null!;

    internal static LocalizedString MaxNudgeString = LanguageEUtil.AddTranslation("MAX NUDGE DISTANCE REACHED");
    internal static float LastNudgeWarning = 0f;
    internal const float NudgeWarningInterval = 1f;

    internal static Vector3 LockedPlacementPosition = Vector3.zero;
    internal static Vector3 InitialLockedPlacementPosition = Vector3.zero;
    internal static Quaternion LockedPlacementRotation = Quaternion.identity;
    internal static bool bPlacementLocked = false;
    public static void OnGadgetSelected(GadgetItem GItem) => ResetLock(GItem, false);
    public static void OnPostGadgetItemUpdate(GadgetItem GItem) 
    {
        CheckInputs(GItem);

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

    public static void SetLockedTransform(GadgetItem GItem)
    {
        GItem._gadgetFootprintInstance.transform.SetPositionAndRotation(LockedPlacementPosition, LockedPlacementRotation);
        GItem._gadgetPlaceholderInstance.transform.SetPositionAndRotation(LockedPlacementPosition, LockedPlacementRotation);
    }

    public static void CheckInputs(GadgetItem GItem)
    {
        if (InputEUtil.OnKeyDown(PreferenceDirector.PlacementLockBind) && GItem._isFootprintVisible && GItem._gadgetDirector.SelectedSlottedGadget != null)
        {
            SetPlacementLocked(GItem, !bPlacementLocked);
        }

        if (!bPlacementLocked) return;

        DoNudge(GItem);

        if(InputEUtil.OnKeyDown(PreferenceDirector.ResetPlacementBind))
        {
            SetPlacementLocked(GItem, false);
        }
    }
    public static void OnGadgetCleared(GadgetItem GItem) => ResetLock(GItem, false);

    public static void ResetLock(GadgetItem GItem, bool bPreservePlacementRotation = false)
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

    public static void DoNudge(GadgetItem GItem)
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

    internal static bool CheckNudgeKey(KeyCode inKey) => (PreferenceDirector.bSmoothNudge && InputEUtil.OnKey(inKey)) || (!PreferenceDirector.bSmoothNudge && InputEUtil.OnKeyDown(inKey));


    public static void SetPlacementLocked(GadgetItem GItem, bool bNewPlacementLocked)
    {
        bPlacementLocked = bNewPlacementLocked;

        if (bNewPlacementLocked)
        {
            LockedPlacementPosition = GItem._gadgetPlaceholderInstance.transform.position;
            InitialLockedPlacementPosition = LockedPlacementPosition;
            LockedPlacementRotation = GItem._gadgetPlaceholderInstance.transform.rotation;
        }
        else ResetLock(GItem, true);
    }
