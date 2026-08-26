using MelonLoader;
using MelonLoader.Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements.Configuration;

public sealed class PreferenceEntry
{
    internal MelonPreferences_Entry<bool> MelonEntry { get; private set; }
    public bool Overridden { get; private set; } = false;
    public bool Value
    {
        get => Overridden ? OverridenValue : MelonEntry.Value;
        private set => OverridenValue = value;
    }
    private bool OverridenValue = false;
    public bool Locked { get; private set; } = false;
    public static implicit operator bool(PreferenceEntry Entry) => Entry.Value;
    internal PreferenceEntry(MelonPreferences_Entry<bool> MelonEntry)
    {
        this.MelonEntry = MelonEntry;
    }
    public void ApplyWithFlags(Configurator.ConfiguratorOperationFlag Flags, bool Other)
    {
        if (Overridden && (Flags & Configurator.ConfiguratorOperationFlag.AllowOverride) == 0)
            return;

        Value = Other;
        Overridden = true;
        Locked |= (Flags & Configurator.ConfiguratorOperationFlag.LockWhenChanged) != 0;
        MelonEntry.IsHidden = Locked;
    }
    public void ClearOverride()
    {
        OverridenValue = false;
        Locked = false;
    }
}