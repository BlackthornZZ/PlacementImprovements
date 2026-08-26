using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements.Configuration;
public class Configurator : IComparable
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
    public readonly ConfiguratorOperationFlag ConfiguratorFlags;
    // Relevant only if the AllowOverride flag is enabled, determines the priority at which 
    public readonly int Priority;
    public readonly string Source;
    public readonly Dictionary<string, bool> ConfigurationOverrides = new();
    public Configurator(string? Source = null, ConfiguratorOperationFlag ConfiguratorFlags = ConfiguratorOperationFlag.Default, ConfiguratorPriority Priority = ConfiguratorPriority.Lowest)
    {
        this.Source = Source ?? Assembly.GetCallingAssembly().GetName().Name ?? "Nameless Assembly";
        this.ConfiguratorFlags = ConfiguratorFlags;
        this.Priority = (int)Priority;
    }

    public void QueueForApplication()
    {
        if (GameContext.Instance != null)
        {
            MelonLogger.Error("Configurator::QueueForApplication called AFTER GameContext initialization! Use Configurator::Reapply instead if this Configurator is already registered.");
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
        if (!PreferenceDirector.ExternalConfigurators.Contains(this))
        {
            MelonLogger.Error("Configurator::Reapply was called on a Configurator that wasn't registered! Register your Configurator with QueueForApplication in OnLateInitialize.");
            return;
        }

        PreferenceDirector.UpdateGlobalConfiguration();
    }

    public int CompareTo(object? obj)
    {
        Configurator? Other = (Configurator?)obj;
        if (Other == null) return 1;

        if (Priority > Other.Priority) return 1;
        if(Priority < Other.Priority) return -1;

        return 0;
    }
}
