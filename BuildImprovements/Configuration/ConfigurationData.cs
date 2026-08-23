using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements.Configuration;

public class ConfigurationEntry
{
    public bool Overridden { get; protected internal set; }
    public virtual bool Value { get; protected internal set; }
    public ConfigurationEntry() { }
    public ConfigurationEntry(bool DefaultValue) => Value = DefaultValue;
    public static implicit operator bool(ConfigurationEntry Entry) => Entry.Value;
    public virtual void ApplyWithFlags(Configurator.ConfiguratorOperationFlag Flags, ConfigurationEntry Other)
    {
        if (Overridden && (Flags & Configurator.ConfiguratorOperationFlag.AllowOverride) == 0)
            return;

        Value |= Other.Value;
        Overridden = true;
    }
}

public struct ConfigurationData
{
    ConfigurationEntry AllowSlopedPlacementAngle;
    ConfigurationEntry IgnorePlayerGroundedState;
    ConfigurationEntry AllowClipping;
    ConfigurationEntry AllowAdvancedMovement;
    ConfigurationEntry AllowPrismacoreGadgets;
    ConfigurationEntry AllowFloatingGadgets;
    ConfigurationEntry InfiniteLinkedRange;
    ConfigurationEntry InvertSmoothNudge;
    ConfigurationEntry AllowScaling;
    ConfigurationEntry NoMinimumPlayerDistance;
}