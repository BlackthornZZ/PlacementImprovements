 // thank you random UnityExplorer crashes after I changed NOTHING.

using BuildImprovements.Input;
using BuildImprovements.Preferences;
using BuildImprovements.UI;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Starlight.Utils;
using UnityEngine;

namespace BuildImprovements.Patches;

[HarmonyPatch(typeof(GadgetItem))]
static class GadgetItemPatches
{
#if !WITH_UNITYEXPLORER

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.UpdateFootprint))]
    private static void UpdateFootprint_Postfix(GadgetItem __instance)
    {
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
        if (PlacementInputDirector.bPlacementLocked)
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