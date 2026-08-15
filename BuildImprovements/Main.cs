using BuildImprovements.Input;
using BuildImprovements.Patches;
using BuildImprovements.Preferences;
using BuildImprovements.UI;
using Il2Cpp;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Expansion;
using Starlight.Managers;
using Starlight.Storage;
using UnityEngine;

namespace BuildImprovements;

[StarlightLoadExpansion]
public sealed class Main : StarlightExpansionV01
{
    public static bool bMultiplayerInstalled { get => StarlightPackageManager.GetPackageInfoFromID("de.pyeight.ranchingtogether") != null; }

    private static PlacementInputDirector? _PlacementInputDirector = null;
    public static PlacementInputDirector PlacementInputDirector
    {
        get 
        {
            if(_PlacementInputDirector == null)
                return _PlacementInputDirector = new();

             return _PlacementInputDirector;
        }
    }

    private static AdditiveUIDirector? _AdditiveUIDirector = null;
    internal static AdditiveUIDirector AdditiveUIDirector 
    {
        get
        {
            if (_AdditiveUIDirector == null)
                return _AdditiveUIDirector = new();
            return _AdditiveUIDirector;
        }
    }

    private static PatchHelper? _PatchHelper = null;
    internal static PatchHelper PatchHelper 
    {
        get 
        {
            if (_PatchHelper == null)
                return _PatchHelper = new();

            return _PatchHelper;
        }
    }
    protected override StarlightPackageInfo info => new()
    {
        ID = BuildInfo.ID,
        Name = BuildInfo.Name,
        Author = BuildInfo.Author,
        CoAuthors = BuildInfo.CoAuthors,
        Contributors = BuildInfo.Contributors,
        Description = BuildInfo.Description,
        SourceCode = BuildInfo.SourceCode,
        Version = BuildInfo.Version,
        Nexus = BuildInfo.Nexus,
        Discord = BuildInfo.Discord,
        UsePrism = BuildInfo.UsePrism,

        LoadTime = ExpansionLoadTime.Startup,
        UnloadTime = ExpansionUnloadTime.Never,
        MultiplayerRequirement = MultiplayerRequirement.ServerAndClient,
        IconPath = "Assets/PlacementImprovementsSmallIcon.png"
    };
    /// <inheritdoc/>
    public override void OnLateInitialize()
    {
        PreferenceDirector.CreatePreferences();
    }

    public override void AfterGameContext(GameContext gameContext)
    {
        gameContext.InputDirector._mainGame.Map.asset.Disable();
        InputRegistrar.RegisterPlacementImprovementsInputs();
        gameContext.InputDirector._mainGame.Map.asset.Enable();
    }

    public override void AfterSceneContext(SceneContext sceneContext)
    {
        _PlacementInputDirector = null;
        _AdditiveUIDirector = null;
        _PatchHelper = null;
    }

    private static TMP_FontAsset GetFont(string fontName) => Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(x => x.name == fontName)!;
}