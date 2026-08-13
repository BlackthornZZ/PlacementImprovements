using BuildImprovements.Input;
using BuildImprovements.Preferences;
using BuildImprovements.UI;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

namespace BuildImprovements.Patches;

[HarmonyPatch(typeof(GadgetItem))]
static class GadgetItemPatches
{

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.UpdateFootprint))]
    private static void UpdateFootprint_Postfix(GadgetItem __instance)
    {
        if(PreferenceDirector.bAllowAdvancedMovement)
            PlacementInputDirector.OnPostGadgetItemFootprintUpdate(__instance);

        PatchHelper.SetGadgetVisuals(PatchHelper.CurrentValidity, __instance);

        // For GadgetItem::PlaceGadgetEvent.
        if (__instance._isPlacementBlocked)
            __instance._isPlacementBlocked = PreferenceDirector.bAllowClipping ? false : true;
        __instance._isPlacementValid |= PreferenceDirector.bAllowSlopedPlacementAngle;
        __instance._isFootprintVisible |= PlacementInputDirector.bPlacementLocked;

        __instance._gadgetDirector._CanPlaceSelectedGadget_k__BackingField.Set(PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid);
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.SetHeldGadget))]
    private static void SetHeldGadget_Postfix(GadgetItem __instance)
    {
        if (PreferenceDirector.bAllowAdvancedMovement)
        {
            PlacementInputDirector.OnGadgetSelected(__instance);
        }

        AdditiveUIDirector.ModifyBottomInputLegendUI(HudUI.Instance.BottomInputLegend);
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
        if(PreferenceDirector.bAllowAdvancedMovement && PlacementInputDirector.bPlacementLocked)
        {
            PlacementInputDirector.SetPlacementLocked(__instance, false);
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.IsInLinkedGadgetRange))]
    private static void IsInLinkedGadgetRange_Postfix(ref bool __result)
    {
        if (PreferenceDirector.bInfiniteLinkedRange) __result = true;
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
        return PrismacoreController._eventDirector.GetRecordEntryForEvent(PrismacoreController._bossFightCompleted.Cast<IGameEvent>()) != null;
    }
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DisableGadgetModeTrigger.OnTriggerEnter))]
    private static bool OnTriggerEnter_Prefix(DisableGadgetModeTrigger __instance)
    {
        return !(PreferenceDirector.bAllowPrismacoreGadgets && IsPrismacoreGadgetModeDisabler(__instance.gameObject) && PrismacoreHarmonized());
    }
};

[HarmonyPatch(typeof(InputDirector))]
static class InputDirectorPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InputDirector.Update))]
    static void Update_Postfix()
    {
        // In menu or instance where gadget-related input shouldnt be accepted
        if (SceneContext.Instance == null || SceneContext.Instance.player == null) 
            return;

        GadgetItem GItem = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem;

        PlacementInputDirector.OnInputDirectorUpdate(SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem);
    }
};