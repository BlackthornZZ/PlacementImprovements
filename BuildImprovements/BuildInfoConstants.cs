#pragma warning disable RCS1110 // Declare type inside namespace
// ReSharper disable once CheckNamespace

internal static class BuildInfo
{
    internal const string ID = "nl.lunarsnail.buildimprovements";
    internal const string Name = "Placement Improvements";
    internal const string Description = "Bypass placement rules and use custom placement colors!";
    internal const string Author = "Lunar Snail";
    internal static readonly string[] CoAuthors = null!;
    internal static readonly string[] Contributors = null!;
    //internal static readonly string[] Contributors = new[] { "1, 2" };
    
    // MelonVersion is shown by ML on startup
    // Version is shown by Starlight
    internal const string MelonVersion = "1.0.0"; 
    internal const string Version = "1.0.0"; // Auto-Dev_Do_not_remove
    internal const string Discord = null!;
    internal const string SourceCode = "https://github.com/BlackthornZZ/PlacementImprovements";
    internal const string Nexus = null!;
    internal const bool UsePrism = false;
    internal const string MinimumStarlightVersion = Starlight.BuildInfo.CodeVersion; // e.g "3.6.3", the min required SR2 version. No beta or alpha versions
    internal const string MinimumGameVersion = "1.2.3"; // e.g 1.1.0 or something similar (optional)
    internal const string ExactGameVersion = "1.2.3"; // e.g 1.1.0 or something similar (optional)
}