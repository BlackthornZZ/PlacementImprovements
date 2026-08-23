using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements.Configuration;
public class Configurator
{
    [Flags]
    public enum ConfiguratorOperationFlag : byte
    {
        // Do not use; here to comply with design rule CA1008: "Enums should have zero value".
        None = 0,
        LockWhenChanged = 1,
        AllowOverride = 2,
        Default = LockWhenChanged | AllowOverride
    }
    public enum ConfiguratorPriority : int
    {
        Lowest = 0,
        Low = 100,
        Mid = 250,
        High = 500,
        VeryHigh = 1000
    }
    private readonly ConfiguratorOperationFlag ConfiguratorFlags;
    // Relevant only if the AllowOverride flag is enabled, determines the priority at which 
    private readonly int Priority;
    public readonly ConfigurationData Configuration = new();
    public Configurator(ConfiguratorOperationFlag ConfiguratorFlags = ConfiguratorOperationFlag.Default, int Priority = 0)
    {
        this.ConfiguratorFlags = ConfiguratorFlags;
        this.Priority = Priority;
    }
    public Configurator(ConfiguratorOperationFlag ConfiguratorFlags = ConfiguratorOperationFlag.Default, ConfiguratorPriority Priority = ConfiguratorPriority.Lowest) : this(ConfiguratorFlags, (int)Priority) { }

    public void QueueForApplication()
    {
        if (GameContext.Instance != null)
        {
            MelonLogger.Error("Configurator::QueueForApplication called AFTER GameContext initialization! Use Configurator::Reapply instead.");
            return;
        }

        if(PreferenceDirector.ExternalConfigurators.Contains(this))
        {
            MelonLogger.Error("Configurator::QueueForApplication was called on a Configurator that was already queued! If this is after GameContext intialization, use Configurator::Reapply instead.");
            return;
        }

        PreferenceDirector.ExternalConfigurators.Add(this);
    }

    public void Reapply()
    {
        throw new NotImplementedException();
    }
}
