using MelonLoader;
using MelonLoader.Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements.Configuration;

internal sealed class PreferenceEntry : ConfigurationEntry
{
    internal MelonPreferences_Entry<bool> MelonEntry;
    public override bool Value
    {
        get => Overridden ? _Value : MelonEntry.Value;
        protected internal set => _Value = value;
    }
    private bool _Value;
    public bool Locked { get; private set; }
    internal PreferenceEntry(MelonPreferences_Entry<bool> MelonEntry) : base(MelonEntry.DefaultValue)
    {
        this.MelonEntry = MelonEntry;
    }
    public override void ApplyWithFlags(Configurator.ConfiguratorOperationFlag Flags, ConfigurationEntry Other)
    {
        base.ApplyWithFlags(Flags, Other);

        Locked |= (Flags & Configurator.ConfiguratorOperationFlag.LockWhenChanged) != 0;
    }
}

internal static class MelonPreferenceCategoryExtensions
{
    internal static PreferenceEntry CreatePreferenceEntry(this MelonPreferences_Category self, string identifier, bool default_value, string? display_name = null, string? description = null, bool is_hidden = false, bool dont_save_default = false, ValueValidator? validator = null)
    {
        MelonPreferences_Entry<bool> MelonEntry = self.CreateEntry<bool>(identifier, default_value, display_name, description, is_hidden, dont_save_default, validator);
        return new PreferenceEntry(MelonEntry);
    }
}