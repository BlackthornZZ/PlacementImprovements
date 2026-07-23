using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using UnityEngine;
using Il2CppMonomiPark.SlimeRancher.Rendering.Pass;
using BuildImprovements.Preferences;

namespace BuildImprovements.Patches;
public enum EGadgetValidity
{
    GV_Invalid, // Wrong (this can sometimes include things the mod considers to be right, when the player has disabled some settings!)
    GV_AlmostValid, // clipping
    GV_Valid // All good
};

// Contains static functions for storing and handling things at a larger scope than just per-patch.
public static class PatchHelper
{
    // Updated with every GadgetItem::IsPlacementValid call.
    internal static bool bTransient_SlopeIsLegal;
    // Backwards compatibility with SR2MP / Ranching Together.
    // SR2MP expects a field with type EGadgetValidity (BindingFlags: Static | Public | NonPublic) and name CurrentValidity.
    // (which is something that existed but is now replaced by a function call)
    // It then uses this field to get the current validity value according to this mod.
    public static EGadgetValidity CurrentValidity { get => GetCurrentValidity(); }
    public static EGadgetValidity GetCurrentValidity(GadgetItem InGadgetItem = null!)
    {
        if(!InGadgetItem)
            InGadgetItem = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem;

        if(!InGadgetItem || !InGadgetItem.enabled)
            return EGadgetValidity.GV_Invalid;

        // Just footprint means we're not placing something, so always valid.
        if (!InGadgetItem._gadgetPlaceholderInstance)
            return EGadgetValidity.GV_Valid;

        if (!InGadgetItem._isGrounded && !PreferenceDirector.bIgnorePlayerGroundedState)
            return EGadgetValidity.GV_Invalid;

        if (!bTransient_SlopeIsLegal && !PreferenceDirector.bAllowSlopedPlacementAngle)
            return EGadgetValidity.GV_Invalid;

        if (!InGadgetItem._gadgetPlaceholderInstanceGadget.IsCompletelyGrounded(InGadgetItem.GadgetItemMetadata.GroundedCheckAdjustment, InGadgetItem.GadgetItemMetadata.GroundedRaycastDistance) && !PreferenceDirector.bAllowFloatingGadgets)
            return EGadgetValidity.GV_Invalid;

        if (InGadgetItem._gadgetPlaceholderInstanceGadget.IsOverlapping(
            InGadgetItem.GadgetItemMetadata.OverlapPlacementFloorTolerance, 
            InGadgetItem.GadgetItemMetadata.GadgetOverlapLayers))
        {
            if (!PreferenceDirector.bAllowClipping)
                return EGadgetValidity.GV_Invalid;
            else
                return EGadgetValidity.GV_AlmostValid;
        }

        return EGadgetValidity.GV_Valid;
    }
    
    public static bool IsSlopeLegal(Vector3 Normal, float MaxValidSlope)
    {
        float Slope = Normal.y;
        // Check for verticality
        if (Math.Abs(Vector3.Dot(Normal, Vector3.up) / Normal.magnitude) <= 0.99)
        {
            double SlopeAngle = Math.Asin(Slope) * 57.29578 + 90;

            if (SlopeAngle >= MaxValidSlope)
            {
                return PreferenceDirector.bAllowSlopedPlacementAngle;
            }
        }

        return true;
    }

    public static void SetGadgetVisuals(EGadgetValidity Validity, GadgetItem InGadgetItem = null!, GadgetsOverlayModeCustomPass Pass = null!)
    {
        if (!InGadgetItem)
            InGadgetItem = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem;
        if (Pass == null)
            Pass = InGadgetItem._gadgetsOverlayCustomPass;

        switch(Validity)
        {
            case EGadgetValidity.GV_Valid:
                SetGadgetPlacementColor(PreferenceDirector.ValidColor, InGadgetItem, Pass);
                break;
            case EGadgetValidity.GV_AlmostValid:
                SetGadgetPlacementColor(PreferenceDirector.AlmostValidColor, InGadgetItem, Pass);
                break;
            case EGadgetValidity.GV_Invalid:
                SetGadgetPlacementColor(PreferenceDirector.InvalidColor, InGadgetItem, Pass, bUseInvalidColor: true);
                break;
        }
    }

    public static void SetGadgetPlacementColor(Color InRGB, GadgetItem InGadgetItem = null!, GadgetsOverlayModeCustomPass GadgetOverlayPass = null!, bool bUseInvalidColor = false)
    {
        if (!InGadgetItem)
            InGadgetItem = SceneContext.Instance.player.GetComponent<PlayerItemController>().GadgetItem;
        if (GadgetOverlayPass == null)
            GadgetOverlayPass = InGadgetItem._gadgetsOverlayCustomPass;

        if (InGadgetItem._gadgetFootprintRendererInstance)
        {
            InRGB.a = InGadgetItem._gadgetFootprintRendererInstance.material.color.a;
            InGadgetItem._gadgetFootprintRendererInstance.material.color = InRGB;
        }

        if (GadgetOverlayPass != null && GadgetOverlayPass._gadgetPlacementMaterial)
        {
            // These will need to be changed if they are adjusted in an update.
            string ColorID = bUseInvalidColor ? "_OverlayInvalidColor" : "_OverlayValidColor";
            Color OverlayValidColor = GadgetOverlayPass._gadgetPlacementMaterial.GetColor(ColorID);
            InRGB.a = OverlayValidColor.a;
            GadgetOverlayPass._gadgetPlacementMaterial.SetColor(ColorID, InRGB);
            GadgetOverlayPass.IsGadgetValid = bUseInvalidColor ? 0f : 1f;
        }
    }
}
