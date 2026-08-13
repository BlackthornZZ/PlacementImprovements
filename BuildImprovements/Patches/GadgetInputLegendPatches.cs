using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildImprovements.Input;
using BuildImprovements.UI;
using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;

namespace BuildImprovements.Patches;

[HarmonyPatch(typeof(GadgetInputLegendUpdater))]
public static class GadgetInputLegendPatches
{
    [HarmonyPatch(nameof(GadgetInputLegendUpdater.UpdateInputLegend))]
    [HarmonyPostfix]
    public static void UpdateInputLegend_Postfix(GadgetInputLegendUpdater __instance)
    {
        if(PlacementInputDirector.bPlacementLocked && __instance._inputLegend == HudUI.Instance.BottomInputLegend)
        {
            __instance._inputLegend.Configure(AdditiveUIDirector.GadgetLockedInputLegend);
        }
    }
}
