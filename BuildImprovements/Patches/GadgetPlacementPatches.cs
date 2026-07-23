// thank you random UnityExplorer crashes after I changed NOTHING.
//#define WITH_UNITYEXPLORER

using AsmResolver.DotNet.Signatures;
using BuildImprovements.Input;
using BuildImprovements.Preferences;
using BuildImprovements.UI;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Il2CppMonomiPark.SlimeRancher.World;
using MelonLoader;
using Starlight.Utils;
using UnityEngine;
using UnityEngine.Localization;

namespace BuildImprovements.Patches;

[HarmonyPatch(typeof(GadgetItem))]
static class GadgetItemPatches
{
#if !WITH_UNITYEXPLORER

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.UpdateFootprint))]
    private static void UpdateFootprint_Postfix(GadgetItem __instance)
    {
        if(PreferenceDirector.bAllowAdvancedMovement)
        PlacementInputDirector.OnPostGadgetItemUpdate(__instance);

        PatchHelper.SetGadgetVisuals(PatchHelper.CurrentValidity, __instance);

        // This code specifically is only relevant for the UpdateFootprint call inside of GadgetItem::PlaceGadgetEvent.
        // To have this run *only* in there however, would require both a prefix and postfix on PlaceGadgetEvent,
        // leading to unnecessary overhead: always running it does not have any side-effects.
        // However, if other mods depend on these variables I could consider adding a compatibility check that ensures this only runs in GadgetItem::PlaceGadgetEvent.
        __instance._isGrounded |= PreferenceDirector.bIgnorePlayerGroundedState;
        if (__instance._isPlacementBlocked)
            __instance._isPlacementBlocked = PreferenceDirector.bAllowClipping ? false : true;
        __instance._isPlacementValid |= PreferenceDirector.bAllowSlopedPlacementAngle;

        __instance._gadgetDirector._CanPlaceSelectedGadget_k__BackingField.Set(PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid);
    }
#endif
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.SetHeldGadget))]
    private static void SetHeldGadget_Postfix(GadgetItem __instance)
    {
        if (PreferenceDirector.bAllowAdvancedMovement)
        {
            PlacementInputDirector.OnGadgetSelected(__instance);
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.IsPlacementValid))]
    private static void IsPlacementValid_Postfix(GadgetItem __instance, Ray ray, RaycastHit hit, ref bool __result)
    {
        if (PreferenceDirector.bAllowAdvancedMovement && PlacementInputDirector.bPlacementLocked)
            PlacementInputDirector.SetLockedTransform(__instance);

        if (__instance.GadgetItemMetadata)
            PatchHelper.bTransient_SlopeIsLegal = PatchHelper.IsSlopeLegal(hit.normal, __instance.GadgetItemMetadata.MaxValidPlacementSlope);

        __instance._gadgetDirector._CanPlaceSelectedGadget_k__BackingField.Set(PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid);

        __result = PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.ClearHeldGadget))]
    private static void ClearHeldGadget_Postfix(GadgetItem __instance)
    {
        if (!PreferenceDirector.bAllowAdvancedMovement) return;

        PlacementInputDirector.OnGadgetCleared(__instance);
    }

    // When Placements are locked we want to unlock them instead of storing the gadget.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GadgetItem.StoreGadget))]
    private static bool StoreGadget_Prefix(GadgetItem __instance)
    {
        if(PlacementInputDirector.bPlacementLocked)
        {
            PlacementInputDirector.SetPlacementLocked(__instance, false);
            return false;
        }
        return true;
    }
};
[HarmonyPatch(typeof(DisableGadgetModeTrigger))]
static class DisableGadgetModeTriggerPatches
{
    // This is terrible practice but I can't think of anything better.
    static bool IsPrismacoreGadgetModeDisabler(GameObject Obj) => Obj.name == "DisableGadgetsVolume" && Obj.scene.name == "zoneLabyrinthCoreBase";
    static bool PrismacoreHarmonized()
    {
        CoreRoomController PrismacoreController = BossFightController.Instance._coreRoomController;
        return PrismacoreController._eventDirector.GetRecordEntryForEvent(InteropStatics.ReinterpretCast<StaticGameEvent, IGameEvent>(PrismacoreController._bossFightCompleted)) != null;
    }
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DisableGadgetModeTrigger.OnTriggerEnter))]
    private static bool OnTriggerEnter_Prefix(DisableGadgetModeTrigger __instance)
    {
        return !(PreferenceDirector.bAllowPrismacoreGadgets && IsPrismacoreGadgetModeDisabler(__instance.gameObject) && PrismacoreHarmonized());
    }
};

