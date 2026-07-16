using BuildImprovements.Preferences;
using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using UnityEngine;

namespace BuildImprovements.Patches;

[HarmonyPatch(typeof(GadgetItem))]
class GadgetItemPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.UpdateFootprint))]
    private static void UpdateFootprint_Postfix(GadgetItem __instance)
    {
        PatchHelper.SetGadgetVisuals(PatchHelper.CurrentValidity, __instance);

        // This code specifically is only relevant for the UpdateFootprint call inside of GadgetItem::PlaceGadgetEvent.
        // To have this run *only* in there however, would require both a prefix and postfix on PlaceGadgetEvent,
        // leading to unnecessary overhead: always running it does not have any side-effects.
        // However, if other mods depend on these variables I could consider adding a compatibility check that ensures this only runs in GadgetItem::PlaceGadgetEvent.
        __instance._isGrounded |= PreferenceDirector.bIgnorePlayerGroundedState;
        if(__instance._isPlacementBlocked)
            __instance._isPlacementBlocked = PreferenceDirector.bAllowClipping ? false : true;
        __instance._isPlacementValid |= PreferenceDirector.bAllowSlopedPlacementAngle;
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.SetHeldGadget))]
    private static void SetHeldGadget_Postfix(GadgetItem __instance)
    {
        InputLegendConfiguration C = HudUI.Instance.BottomInputLegend._configuration;
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.IsPlacementValid))]
    private static void IsPlacementValid_Postfix(GadgetItem __instance, Ray ray, RaycastHit hit, ref bool __result)
    {
        if(__instance.GadgetItemMetadata)
            PatchHelper.bTransient_SlopeIsLegal = PatchHelper.IsSlopeLegal(hit.normal, __instance.GadgetItemMetadata.MaxValidPlacementSlope);

        __result = PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid;
    }
};
