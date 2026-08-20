using BuildImprovements.Input;
using BuildImprovements.Configuration;
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
            Main.PlacementInputDirector.OnPostGadgetItemFootprintUpdate();

        Main.PatchHelper.SetGadgetVisuals(Main.PatchHelper.CurrentValidity, __instance);

        // For GadgetItem::PlaceGadgetEvent.
        if (__instance._isPlacementBlocked)
            __instance._isPlacementBlocked = PreferenceDirector.bAllowClipping ? false : true;
        __instance._isPlacementValid |= PreferenceDirector.bAllowSlopedPlacementAngle;
        __instance._isFootprintVisible |= Main.PlacementInputDirector.bPlacementLocked;

        __instance._gadgetDirector._CanPlaceSelectedGadget_k__BackingField.Set(Main.PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid);
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.SetHeldGadget))]
    private static void SetHeldGadget_Postfix(GadgetItem __instance)
    {
        if (PreferenceDirector.bAllowAdvancedMovement)
        {
            Main.PlacementInputDirector.OnGadgetSelected();
        }

        Main.AdditiveUIDirector.ModifyBottomInputLegendUI(HudUI.Instance.BottomInputLegend);
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.IsPlacementValid))]
    private static void IsPlacementValid_Postfix(GadgetItem __instance, Ray ray, RaycastHit hit, ref bool __result)
    {
        if (PreferenceDirector.bAllowAdvancedMovement && Main.PlacementInputDirector.bPlacementLocked)
            Main.PlacementInputDirector.SetLockedTransform();

        if (__instance.GadgetItemMetadata)
            Main.PatchHelper.bTransient_SlopeIsLegal = Main.PatchHelper.IsSlopeLegal(hit.normal, __instance.GadgetItemMetadata.MaxValidPlacementSlope);

        __instance._gadgetDirector._CanPlaceSelectedGadget_k__BackingField.Set(Main.PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid);

        __result = Main.PatchHelper.CurrentValidity != EGadgetValidity.GV_Invalid;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GadgetItem.ClearHeldGadget))]
    private static void ClearHeldGadget_Postfix()
    {
        if (!PreferenceDirector.bAllowAdvancedMovement) return;

        Main.PlacementInputDirector.OnGadgetCleared();
    }

    // When Placements are locked we want to unlock them instead of storing the gadget.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GadgetItem.StoreGadget))]
    private static bool StoreGadget_Prefix()
    {
        if(PreferenceDirector.bAllowAdvancedMovement && Main.PlacementInputDirector.bPlacementLocked)
        {
            Main.PlacementInputDirector.SetPlacementLocked(false);
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

        Main.PlacementInputDirector.OnInputDirectorUpdate();
    }
};