using BuildImprovements.Input;
using BuildImprovements.Preferences;
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
    };
    /// <inheritdoc/>
    public override void OnLateInitialize()
    {
        PreferenceDirector.CreatePreferences();
    }

    public override void AfterGameContext(GameContext gameContext)
    {
        PlacementInputDirector.InitInputs();
    }

    private static TMP_FontAsset GetFont(string fontName) => Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(x => x.name == fontName)!;
}